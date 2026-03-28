using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers;

/// <summary>
/// Controller xử lý các yêu cầu liên quan đến xác thực người dùng, bao gồm đăng nhập, đăng ký và đăng xuất.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("AuthController is working!");
    }
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