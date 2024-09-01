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
        public void GetFilteredOperationLists()
        {
            // Arrange
            int tankID = 1;
            int fuelID = 1;
            var fuelsContextMock = new Mock<FuelsContext>();

            var operations = TestDataHelper.GetFakeOperationsList()
                .Where(op => op.TankID == tankID)
                .Where(op=>op.FuelID==fuelID).ToList();
            fuelsContextMock.Setup(x => x.Operations).ReturnsDbSet(operations);

            //Act
            OperationsController OperationsController = new(fuelsContextMock.Object);
            var result = OperationsController.GetFilteredOperations(fuelID,tankID);

            // Assert
            var viewResult = Assert.IsType<List<OperationViewModel>>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(operations.Count, result.Count);
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
            Assert.IsType<ObjectResult>(foundResult);
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
            controller.ModelState.AddModelError("error", "some error");
            var result = controller.Post(operation: null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
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
            Assert.IsType<OkObjectResult>(result);
        }



        [Fact]
        public void Put_ReturnsNotFoundResult()
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
            controller.ModelState.AddModelError("error", "some error");
            var result = controller.Put(operation);

            // Assert
            Assert.IsType<NotFoundResult>(result);
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
            Assert.IsType<OkObjectResult>(result);
            fuelsContextMock.Verify();
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
            Assert.IsType<OkObjectResult>(result);
            fuelsContextMock.Verify();

        }

    }

}