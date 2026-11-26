using FuelStationWebApi.Controllers;
using FuelStationWebApi.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Tests
{
    /// <summary>
    /// Тесты проверки данных запроса входа.
    /// Эта проверка выполняется до обращения к UserManager и AuthService, поэтому вместо них передается null.
    /// Проверка пароля и выдача токена здесь не проверяются: для них нужна база данных пользователей,
    /// и их проверяют запросы раздела 0 файла WebAPITest.http.
    /// </summary>
    public class AuthControllerTest
    {
        private static AuthController CreateController() => new(userManager: null!, authService: null!);

        [Fact]
        public async Task Post_NullModel_ReturnsBadRequest()
        {
            // Act
            var result = await CreateController().Post(model: null!);

            // Assert
            // пустое тело запроса отклоняется, а не приводит к необработанному исключению (ответ 500)
            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(null, "_Aa123456")]
        [InlineData("", "_Aa123456")]
        [InlineData("admin@gmail.com", null)]
        [InlineData("admin@gmail.com", "")]
        public async Task Post_MissingEmailOrPassword_ReturnsBadRequest(string? email, string? password)
        {
            // Arrange
            var model = new LoginViewModel { Email = email, Password = password };

            // Act
            var result = await CreateController().Post(model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
