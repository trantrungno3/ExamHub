using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
            [HttpPost("login")]
            public IActionResult Login()
            {
                // Implement login logic here
                return Ok("Login successful");
            }

            [HttpPost("register")]
            public IActionResult Register()
            {
                // Implement registration logic here
                return Ok("Registration successful");
            }

            [HttpPost("logout")]
            public IActionResult Logout()
            {
                // Implement logout logic here
                return Ok("Logout successful");
            }
    }
}
