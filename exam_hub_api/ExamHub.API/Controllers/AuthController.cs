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
    /// <param name="dto">Tên đăng nhập và mật khẩu.</param>
    /// <returns>Access token + refresh token nếu đăng nhập thành công.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<RequestResponse<TokenModel>>> Login([FromBody] LoginDto dto)
    {
        return Ok(await service.Login(dto));
    }

    /// <summary>Đăng ký tài khoản mới</summary>
    /// <param name="dto">Thông tin tài khoản cần đăng ký.</param>
    /// <returns>Kết quả đăng ký.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RequestResponse<object>>> Register([FromBody] RegisterDto dto)
    {
        return Ok(await service.Register(dto));
    }

    /// <summary>Làm mới access token bằng refresh token</summary>
    /// <param name="dto">Access token + refresh token hiện tại.</param>
    /// <returns>Access token mới.</returns>
    [AllowAnonymous]
    [HttpGet("refresh-token")]
    public async Task<ActionResult<RequestResponse<string>>> RefreshToken([FromQuery] TokenModel dto)
    {
        return Ok(await service.RefreshToken(dto));
    }

    /// <summary>Lấy thông tin tài khoản đang đăng nhập</summary>
    /// <returns>Thông tin tài khoản hiện tại.</returns>
    [HttpGet("info")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<UserInfo>>> GetInfo()
    {
        return Ok(await service.GetUserInfo(User.GetUserName()));
    }

    /// <summary>Cập nhật thông tin cá nhân của tài khoản đang đăng nhập</summary>
    /// <param name="dto">Thông tin cá nhân mới.</param>
    /// <returns>Thông tin tài khoản sau khi cập nhật.</returns>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<UserInfo>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        return Ok(await service.UpdateProfile(User.GetUserName(), dto));
    }

    /// <summary>Đổi mật khẩu của tài khoản đang đăng nhập</summary>
    /// <param name="dto">Mật khẩu cũ và mật khẩu mới.</param>
    /// <returns>Kết quả đổi mật khẩu.</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        return Ok(await service.ChangePassword(User.GetUserName(), dto));
    }
}
