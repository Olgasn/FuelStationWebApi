using FuelStationWebApi.Models;
using System.Text.Json;

namespace Tests
{
    /// <summary>
    /// Тесты представления класса Operation в формате JSON.
    /// Контроллер принимает и возвращает объект Operation целиком, поэтому от его JSON-представления зависит,
    /// какие данные клиент может изменить запросом. Вложенные объекты fuel и tank не должны приниматься:
    /// иначе Entity Framework создал бы или изменил записи справочников вместе с операцией.
    /// </summary>
    public class OperationJsonTest
    {
        // настройки, которые ASP.NET Core по умолчанию использует для веб-приложений (имена свойств со строчной буквы)
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        [Fact]
        public void Deserialize_IgnoresNestedFuelAndTank()
        {
            // Arrange
            string json = """
                {
                    "fuelID": 2, "tankID": 24, "inc_Exp": 1, "date": "2026-11-26",
                    "fuel": { "fuelType": "новый вид топлива", "fuelDensity": 1 },
                    "tank": { "tankID": 24, "tankType": "другое название" }
                }
                """;

            // Act
            var operation = JsonSerializer.Deserialize<Operation>(json, Options);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal(2, operation.FuelID);
            Assert.Equal(24, operation.TankID);
            // вложенные объекты не привязываются к свойствам Fuel и Tank
            Assert.Null(operation.Fuel);
            Assert.Null(operation.Tank);
        }

        [Fact]
        public void Serialize_DoesNotWriteNavigationProperties()
        {
            // Arrange
            var fuel = new Fuel { FuelID = 2, FuelType = "Бензин Аи-95" };
            var operation = new Operation { OperationID = 1, FuelID = 2, TankID = 3, Fuel = fuel, Tank = new Tank { TankID = 3 } };
            // двусторонняя связь, как у объектов, загруженных Entity Framework:
            // без атрибута JsonIgnore сериализация зациклилась бы (операция - топливо - операции - ...)
            fuel.Operations.Add(operation);

            // Act
            string json = JsonSerializer.Serialize(operation, Options);

            // Assert
            using var document = JsonDocument.Parse(json);
            Assert.False(document.RootElement.TryGetProperty("fuel", out _));
            Assert.False(document.RootElement.TryGetProperty("tank", out _));
            Assert.Equal(2, document.RootElement.GetProperty("fuelID").GetInt32());
            Assert.Equal(3, document.RootElement.GetProperty("tankID").GetInt32());
        }
    }
}
