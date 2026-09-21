using Microsoft.AspNetCore.RateLimiting;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataAccessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using TVT.Core.Enums;
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
    private const string RefreshCookie = "examhub_refresh";

    // Path bó hẹp về /api/Auth: browser chỉ gửi refresh token tới đúng các endpoint auth, không
    // kèm nó vào mọi request API khác.
    private static CookieOptions CookieSettings(TimeSpan? maxAge = null) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = "/api/Auth",
        MaxAge = maxAge,
    };

    private void SetRefreshCookie(string token)
        => Response.Cookies.Append(RefreshCookie, token, CookieSettings(TimeSpan.FromDays(30)));

    private void ClearRefreshCookie()
        => Response.Cookies.Delete(RefreshCookie, CookieSettings());

    /// <summary>Đưa refresh token vào cookie HttpOnly và chỉ trả access token ra body.</summary>
    private ActionResult<RequestResponse<AccessTokenResponse>> AccessTokenOnly(
        RequestResponse<TokenModel> result)
    {
        if (result.Status != RequestResponseStatus.Success || result.Data is null)
            return Ok(RequestResponse<AccessTokenResponse>.Error(result.Message ?? "Đăng nhập thất bại."));

        SetRefreshCookie(result.Data.RefreshToken);
        return Ok(RequestResponse<AccessTokenResponse>.Success(
            result.Message, new AccessTokenResponse(result.Data.AccessToken), 1));
    }

    /// <summary>Đăng nhập và nhận JWT token</summary>
    /// <param name="dto">Tên đăng nhập và mật khẩu.</param>
    /// <returns>Access token; refresh token đi bằng cookie HttpOnly.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<RequestResponse<AccessTokenResponse>>> Login([FromBody] LoginDto dto)
    {
        return AccessTokenOnly(await service.Login(dto));
    }

    /// <summary>Đăng ký tài khoản mới</summary>
    /// <param name="dto">Thông tin tài khoản cần đăng ký.</param>
    /// <returns>Kết quả đăng ký.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<RequestResponse<object>>> Register([FromBody] RegisterDto dto)
    {
        return Ok(await service.Register(dto));
    }

    /// <summary>Làm mới access token và rotate refresh token</summary>
    /// <param name="dto">Access token hiện tại; refresh token đọc từ cookie HttpOnly.</param>
    /// <returns>Access token mới; cookie refresh được rotate.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<RequestResponse<AccessTokenResponse>>> RefreshToken(
        [FromBody] RefreshAccessTokenRequest dto)
    {
        var refreshToken = Request.Cookies[RefreshCookie];
        if (string.IsNullOrEmpty(refreshToken))
            return Ok(RequestResponse<AccessTokenResponse>.Error("Phiên đăng nhập đã hết hạn."));

        return AccessTokenOnly(await service.RefreshToken(dto.AccessToken, refreshToken));
    }

    /// <summary>Đăng xuất: thu hồi refresh token đã lưu và xoá cookie</summary>
    /// <returns>Kết quả đăng xuất.</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<bool>>> Logout()
    {
        var result = await service.RevokeRefreshToken(User.GetUserName());
        ClearRefreshCookie();
        return Ok(result);
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
