using FuelStationWebApi.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    /// <summary>
    /// Тесты начального набора данных (DbInitializer).
    /// Используется настоящая СУБД SQLite, но в памяти: база создается заново для каждого теста
    /// (xUnit создает новый объект класса для каждого теста) и не затрагивает файл fuels.db.
    ///
    /// Начальные данные используются в других местах проекта, поэтому их изменения здесь не случайны:
    /// HTML-страницы берут код записи из номера выбранного пункта списка (он должен идти подряд с 1),
    /// а в WebAPITest.http записаны коды емкостей 24 и 5, топлива 2 и 3 и количества записей.
    /// При изменении DbInitializer нужно обновить и эти тесты, и WebAPITest.http.
    /// </summary>
    public class DbInitializerTest : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly FuelsContext _db;

        public DbInitializerTest()
        {
            // база в памяти существует, пока открыто соединение
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<FuelsContext>().UseSqlite(_connection).Options;
            _db = new FuelsContext(options);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public void Initialize_FillsAllTables()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            Assert.Equal(14, _db.Fuels.Count());
            Assert.Equal(35, _db.Tanks.Count());
            Assert.Equal(300, _db.Operations.Count());
        }

        [Fact]
        public void Initialize_AssignsIdsInInsertionOrderStartingFromOne()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            // HTML-страницы считают, что код записи равен номеру пункта выпадающего списка
            Assert.Equal(Enumerable.Range(1, 14), _db.Fuels.OrderBy(f => f.FuelID).Select(f => f.FuelID));
            Assert.Equal(Enumerable.Range(1, 35), _db.Tanks.OrderBy(t => t.TankID).Select(t => t.TankID));
            Assert.Equal(Enumerable.Range(1, 300), _db.Operations.OrderBy(o => o.OperationID).Select(o => o.OperationID));
        }

        [Fact]
        public void Initialize_KeepsFuelsAndTanksReferencedByWebApiTestHttp()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            // WebAPITest.http: фильтр FuelID=2&TankID=24 и FuelID=3&TankID=5 должен возвращать данные
            foreach (var (fuelID, tankID) in new[] { (2, 24), (3, 5) })
            {
                Assert.True(_db.Operations.Any(o => o.FuelID == fuelID && o.TankID == tankID),
                    $"нет операций с топливом {fuelID} в емкости {tankID}");
            }
        }

        [Fact]
        public void Initialize_UsesRealNamesAndPositiveParameters()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            Assert.All(_db.Fuels, f =>
            {
                Assert.False(string.IsNullOrWhiteSpace(f.FuelType));
                Assert.InRange(f.FuelDensity, 0.1F, 2F);   // кг/л
            });
            Assert.All(_db.Tanks, t =>
            {
                Assert.False(string.IsNullOrWhiteSpace(t.TankType));
                Assert.False(string.IsNullOrWhiteSpace(t.TankMaterial));
                Assert.True(t.TankVolume > 0);
                Assert.True(t.TankWeight > 0);
            });
            // названия топлива и емкостей не повторяются, иначе в выпадающих списках их не различить
            Assert.Equal(_db.Fuels.Count(), _db.Fuels.Select(f => f.FuelType).Distinct().Count());
            Assert.Equal(_db.Tanks.Count(), _db.Tanks.Select(t => t.TankType).Distinct().Count());
        }

        [Fact]
        public void Initialize_LinksEveryOperationToExistingFuelAndTank()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            var operations = _db.Operations.Include(o => o.Fuel).Include(o => o.Tank).ToList();
            Assert.All(operations, o =>
            {
                Assert.NotNull(o.Fuel);
                Assert.NotNull(o.Tank);
                Assert.NotNull(o.Inc_Exp);
            });
        }

        [Fact]
        public void Initialize_EachTankStoresOnlyOneFuel()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            // по условию емкость хранит один вид топлива, поэтому все ее операции относятся к одному топливу
            var fuelsPerTank = _db.Operations
                .GroupBy(o => o.TankID)
                .Select(g => g.Select(o => o.FuelID).Distinct().Count())
                .ToList();
            Assert.All(fuelsPerTank, count => Assert.Equal(1, count));
        }

        [Fact]
        public void Initialize_KeepsTankStockBetweenZeroAndVolume()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            // остаток в емкости - сумма ее приходов и расходов по порядку операций -
            // не может быть отрицательным и превышать объем емкости
            var tanks = _db.Tanks.ToDictionary(t => t.TankID);
            var operationsByTank = _db.Operations.OrderBy(o => o.OperationID).ToList().GroupBy(o => o.TankID);
            foreach (var tankOperations in operationsByTank)
            {
                float volume = tanks[tankOperations.Key].TankVolume;
                float stock = 0;
                foreach (var operation in tankOperations)
                {
                    Assert.NotNull(operation.Inc_Exp);
                    stock += operation.Inc_Exp.Value;
                    Assert.True(stock >= 0,
                        $"емкость {tankOperations.Key}: остаток {stock} л после операции {operation.OperationID} отрицателен");
                    Assert.True(stock <= volume,
                        $"емкость {tankOperations.Key}: остаток {stock} л после операции {operation.OperationID} больше объема {volume} л");
                }
            }
        }

        [Fact]
        public void Initialize_CreatesOperationsInChronologicalOrderForLastThreeMonths()
        {
            // Act
            DbInitializer.Initialize(_db);

            // Assert
            var today = DateTime.Now.Date;
            var dates = _db.Operations.OrderBy(o => o.OperationID).Select(o => o.Date).ToList();
            // код операции растет вместе с ее датой
            Assert.Equal(dates.OrderBy(d => d), dates);
            // операции выполнены за последние 90 суток (не раньше и не позже)
            Assert.All(dates, d => Assert.InRange(d, today.AddDays(-90), today.AddDays(-1)));
        }

        [Fact]
        public void Initialize_IsRepeatable()
        {
            // Act
            DbInitializer.Initialize(_db);
            var firstOperations = _db.Operations.OrderBy(o => o.OperationID)
                .Select(o => new { o.TankID, o.FuelID, o.Inc_Exp }).ToList();
            // повторный запуск (например, при следующем старте приложения) не должен менять данные
            DbInitializer.Initialize(_db);

            // Assert
            Assert.Equal(14, _db.Fuels.Count());
            Assert.Equal(35, _db.Tanks.Count());
            Assert.Equal(firstOperations,
                _db.Operations.OrderBy(o => o.OperationID).Select(o => new { o.TankID, o.FuelID, o.Inc_Exp }).ToList());
        }

        [Fact]
        public void Initialize_DoesNotTouchNonEmptyDatabase()
        {
            // Arrange
            // в базе уже есть данные пользователя: приложение не должно их дополнять начальными
            _db.Database.EnsureCreated();
            _db.Fuels.Add(new FuelStationWebApi.Models.Fuel { FuelType = "Свое топливо", FuelDensity = 0.8F });
            _db.SaveChanges();

            // Act
            DbInitializer.Initialize(_db);

            // Assert
            Assert.Equal("Свое топливо", Assert.Single(_db.Fuels).FuelType);
            Assert.Empty(_db.Tanks);
            Assert.Empty(_db.Operations);
        }
    }
}
