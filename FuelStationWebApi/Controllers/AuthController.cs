using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using FuelStationWebApi.Services;
using FuelStationWebApi.ViewModels;
namespace FuelStationWebApi.Controllers
{

    [Route("api/[controller]")]
    public class AuthController(UserManager<IdentityUser> userManager, AuthService authService) : Controller
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly AuthService _authService = authService;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = _authService.GenerateToken(user);
                              
                return Ok(token);
            }

            return Unauthorized();
        }
    }

}
