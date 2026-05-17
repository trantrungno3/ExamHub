using ExamHub.Core.Application.Services;
using ExamHub.Core.DataAccessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using TVT.Core.Extensions;
using TVT.Core.IdentityUser.PostgreSql.Models;
using TVT.Core.Models;

namespace ExamHub.API.Controllers;

/// <summary>
/// Controller xử lý các yêu cầu liên quan đến xác thực người dùng, bao gồm đăng nhập, đăng ký và đăng xuất.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService service) : ControllerBase
{
    /// <summary>Đăng nhập và nhận JWT token</summary>
    [HttpPost("login")]
    public async Task<ActionResult<RequestResponse<TokenModel>>> Login([FromBody] LoginDto dto)
    {
        return Ok(await service.Login(dto));
    }

    /// <summary>Đăng ký tài khoản mới</summary>
    [HttpPost("register")]
    public async Task<ActionResult<RequestResponse<object>>> Register([FromBody] RegisterDto dto)
    {
        return Ok(await service.Register(dto));
    }

    /// <summary>Làm mới access token bằng refresh token</summary>
    [HttpGet("refresh-token")]
    public async Task<ActionResult<RequestResponse<string>>> RefreshToken([FromQuery] TokenModel dto)
    {
        return Ok(await service.RefreshToken(dto));
    }

    /// <summary>Lấy thông tin tài khoản đang đăng nhập</summary>
    [HttpGet("info")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<UserInfo>>> GetInfo()
    {
        return Ok(await service.GetUserInfo(User.GetUserName()));
    }
}
