using FuelStationWebApi.Data;
using FuelStationWebApi.Models;
using FuelStationWebApi.ViewModels;
using Microsoft.AspNetCore.Mvc;

using Moq.EntityFrameworkCore;
namespace Tests
{
    public class OperationsControllerTest
    {
        [Fact]
        public void GetOperationList()
        {
            // Arrange
            var fuelsContextMock = new Mock<FuelsContext>();
            var operations = TestDataHelper.GetFakeOperationsList();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            //Act
            OperationsController OperationsController = new(fuelsContextMock.Object);
            var result = OperationsController.Get();

            // Assert
            var viewResult = Assert.IsType<List<OperationViewModel>>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(operations.Count, result.Count);
        }

        [Fact]
        public void GetOperationList_MapsOperationToViewModel()
        {
            // Arrange
            var fuelsContextMock = new Mock<FuelsContext>();
            var operations = TestDataHelper.GetFakeOperationsList();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            //Act
            var result = new OperationsController(fuelsContextMock.Object).Get();

            // Assert
            // в модель представления попадают названия топлива и емкости из связанных таблиц
            foreach (var operation in operations)
            {
                var model = Assert.Single(result, m => m.OperationID == operation.OperationID);
                Assert.Equal(operation.FuelID, model.FuelID);
                Assert.Equal(operation.Fuel.FuelType, model.FuelType);
                Assert.Equal(operation.TankID, model.TankID);
                Assert.Equal(operation.Tank.TankType, model.TankType);
                Assert.Equal(operation.Inc_Exp, model.Inc_Exp);
                Assert.Equal(operation.Date, model.Date);
            }
        }

        // Фильтрация выполняется самим контроллером, поэтому в контекст передается полный список операций,
        // а ожидаемый результат вычисляется независимо от контроллера.
        // Код 0 означает "не фильтровать по этому полю".
        private static (List<Operation> All, List<OperationViewModel> Result) Filter(int fuelID, int tankID)
        {
            var fuelsContextMock = new Mock<FuelsContext>();
            var operations = TestDataHelper.GetFakeOperationsList();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            var result = new OperationsController(fuelsContextMock.Object).GetFilteredOperations(fuelID, tankID);
            return (operations, result);
        }

        [Fact]
        public void GetFilteredOperationLists()
        {
            // Act
            var (operations, result) = Filter(fuelID: 0, tankID: 0);

            // Assert
            // без фильтра возвращаются все операции
            var viewResult = Assert.IsType<List<OperationViewModel>>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(operations.Count, result.Count);
        }

        [Fact]
        public void GetFilteredOperations_ByTank_ReturnsOnlyOperationsOfThatTank()
        {
            // Arrange
            int tankID = TestDataHelper.GetFakeOperationsList()[0].TankID;

            // Act
            var (operations, result) = Filter(fuelID: 0, tankID: tankID);

            // Assert
            var expected = operations.Where(o => o.TankID == tankID).Select(o => o.OperationID).ToList();
            Assert.NotEmpty(expected);
            Assert.True(expected.Count < operations.Count, "тест ничего не проверяет: фильтр не отсекает ни одной операции");
            Assert.Equal(expected, result.Select(o => o.OperationID));
        }

        [Fact]
        public void GetFilteredOperations_ByFuel_ReturnsOnlyOperationsOfThatFuel()
        {
            // Arrange
            int fuelID = TestDataHelper.GetFakeOperationsList()[0].FuelID;

            // Act
            var (operations, result) = Filter(fuelID: fuelID, tankID: 0);

            // Assert
            var expected = operations.Where(o => o.FuelID == fuelID).Select(o => o.OperationID).ToList();
            Assert.NotEmpty(expected);
            Assert.True(expected.Count < operations.Count, "тест ничего не проверяет: фильтр не отсекает ни одной операции");
            Assert.Equal(expected, result.Select(o => o.OperationID));
        }

        [Fact]
        public void GetFilteredOperations_ByFuelAndTank_ReturnsOperationsMatchingBoth()
        {
            // Arrange
            var first = TestDataHelper.GetFakeOperationsList()[0];

            // Act
            var (operations, result) = Filter(fuelID: first.FuelID, tankID: first.TankID);

            // Assert
            var expected = operations
                .Where(o => o.FuelID == first.FuelID && o.TankID == first.TankID)
                .Select(o => o.OperationID).ToList();
            Assert.NotEmpty(expected);
            Assert.True(expected.Count < operations.Count, "тест ничего не проверяет: фильтр не отсекает ни одной операции");
            Assert.Equal(expected, result.Select(o => o.OperationID));
        }

        [Fact]
        public void GetFilteredOperations_NoMatches_ReturnsEmptyList()
        {
            // Act
            var (_, result) = Filter(fuelID: 99, tankID: 99);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetTanks()
        {
            // Arrange
            var fuelsContextMock = new Mock<FuelsContext>();
            var tanks = TestDataHelper.GetFakeTanksList();
            fuelsContextMock.Setup(x => x.Tanks).ReturnsDbSet(tanks);

            //Act
            OperationsController OperationsController = new(fuelsContextMock.Object);
            var result = OperationsController.GetTanks();

            // Assert
            var viewResult = Assert.IsType<List<Tank>>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(tanks.Count, viewResult.Count);
            Assert.Equal(tanks.Select(t => t.TankID), viewResult.Select(t => t.TankID));
        }
        [Fact]
        public void GetFuels()
        {
            // Arrange
            var fuelsContextMock = new Mock<FuelsContext>();
            var fuels = TestDataHelper.GetFakeFuelsList();
            fuelsContextMock.Setup(x => x.Fuels).ReturnsDbSet(fuels);

            //Act
            OperationsController OperationsController = new(fuelsContextMock.Object);
            var result = OperationsController.GetFuels();

            // Assert
            var viewResult = Assert.IsType<List<Fuel>>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(fuels.Count, viewResult.Count);
            Assert.Equal(fuels.Select(f => f.FuelID), viewResult.Select(f => f.FuelID));
        }

        [Fact]
        public void GetOperation()
        {
            // Arrange
            var fuelsContextMock = new Mock<FuelsContext>();
            var operations = TestDataHelper.GetFakeOperationsList();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var notFoundResult = controller.Get(400);
            var foundResult = controller.Get(1);

            // Assert
            Assert.IsType<NotFoundResult>(notFoundResult);
            var objectResult = Assert.IsType<ObjectResult>(foundResult);
            // возвращается именно запрошенная операция
            var operation = Assert.IsType<Operation>(objectResult.Value);
            Assert.Equal(1, operation.OperationID);
        }


        [Fact]
        public void Post_ReturnsBadRequestResult()
        {
            // Arrange
            var operations = TestDataHelper.GetFakeOperationsList();
            var fuelsContextMock = new Mock<FuelsContext>();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var result = controller.Post(operation: null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            // при пустом теле запроса изменения в базу не сохраняются
            fuelsContextMock.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Fact]
        public void Post_ReturnsOkObjectResult()
        {
            // Arrange
            var operations = TestDataHelper.GetFakeOperationsList();
            var fuelsContextMock = new Mock<FuelsContext>();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);
            Operation operation = new()
            {
                OperationID = 200,
                FuelID = 3,
                TankID = 3,
                Date = DateTime.Now,
                Inc_Exp = -3.13F,
                Fuel = TestDataHelper.GetFakeFuelsList().SingleOrDefault(m => m.FuelID == 3),
                Tank = TestDataHelper.GetFakeTanksList().SingleOrDefault(m => m.TankID == 3)
            };

            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var result = controller.Post(operation);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(operation, okResult.Value);
            // операция добавлена в таблицу и изменения сохранены ровно один раз
            // (ReturnsDbSet не меняет исходный список, поэтому проверяем вызов Add у таблицы-заглушки)
            Mock.Get(fuelsContextMock.Object.Operations).Verify(x => x.Add(operation), Times.Once());
            fuelsContextMock.Verify(x => x.SaveChanges(), Times.Once());
        }



        [Fact]
        public void Put_ReturnsBadRequestResult()
        {
            // Arrange
            var operations = TestDataHelper.GetFakeOperationsList();
            var fuelsContextMock = new Mock<FuelsContext>();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var result = controller.Put(operation: null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            fuelsContextMock.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Fact]
        public void Put_ReturnsNotFoundResult()
        {
            // Arrange
            var operations = TestDataHelper.GetFakeOperationsList();
            var fuelsContextMock = new Mock<FuelsContext>();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);
            // операции с кодом 200 в таблице нет
            Operation operation = new()
            {
                OperationID = 200,
                FuelID = 3,
                TankID = 3,
                Date = DateTime.Now,
                Inc_Exp = -3.13F,
                Fuel = TestDataHelper.GetFakeFuelsList().SingleOrDefault(m => m.FuelID == 3),
                Tank = TestDataHelper.GetFakeTanksList().SingleOrDefault(m => m.TankID == 3)
            };


            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var result = controller.Put(operation);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            fuelsContextMock.Verify(x => x.SaveChanges(), Times.Never());
        }


        [Fact]
        public void Put_ReturnsOkObjectResult()
        {
            // Arrange
            var operations = TestDataHelper.GetFakeOperationsList();
            var fuelsContextMock = new Mock<FuelsContext>();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);
            Operation operation = new()
            {
                OperationID = 1,
                FuelID = 3,
                TankID = 3,
                Date = DateTime.Now,
                Inc_Exp = -3.13F,
                Fuel = TestDataHelper.GetFakeFuelsList().SingleOrDefault(m => m.FuelID == 3),
                Tank = TestDataHelper.GetFakeTanksList().SingleOrDefault(m => m.TankID == 3)
            };
            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var result = controller.Put(operation);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(operation, okResult.Value);
            // изменяется именно переданная операция, изменения сохраняются ровно один раз
            fuelsContextMock.Verify(x => x.Update(operation), Times.Once());
            fuelsContextMock.Verify(x => x.SaveChanges(), Times.Once());
        }


        [Fact]
        public void Delete_ReturnsNotFound()
        {
            // Arrange
            var operations = TestDataHelper.GetFakeOperationsList();
            var fuelsContextMock = new Mock<FuelsContext>();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var notFoundResult = controller.Delete(40);


            // Assert
            Assert.IsType<NotFoundResult>(notFoundResult);
            Mock.Get(fuelsContextMock.Object.Operations).Verify(x => x.Remove(It.IsAny<Operation>()), Times.Never());
            fuelsContextMock.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Fact]
        public void Delete_ReturnsOkObjectResult()
        {
            // Arrange
            var operations = TestDataHelper.GetFakeOperationsList();
            var fuelsContextMock = new Mock<FuelsContext>();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);


            // Act
            var controller = new OperationsController(fuelsContextMock.Object);
            var result = controller.Delete(3);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // возвращается удаленная операция, из таблицы она исчезает, изменения сохраняются ровно один раз
            var deleted = Assert.IsType<Operation>(okResult.Value);
            Assert.Equal(3, deleted.OperationID);
            Mock.Get(fuelsContextMock.Object.Operations).Verify(x => x.Remove(deleted), Times.Once());
            fuelsContextMock.Verify(x => x.SaveChanges(), Times.Once());

        }

    }

}
