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
    /// <summary>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<RequestResponse<TokenModel>> Login([FromBody] LoginDto dto)
    {
        return await service.Login(dto);
    }

    /// <summary>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<RequestResponse<object>> Register([FromBody] RegisterDto dto)
    {
        return await service.Register(dto);
    }

    /// <summary>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpGet("refresh-token")]
    public async Task<RequestResponse<string>> RefreshToken([FromQuery] TokenModel dto)
    {
        return await service.RefreshToken(dto);
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("info")]
    [Authorize]
    public async Task<RequestResponse<UserInfo>> GetInfo()
    {
        return await service.GetUserInfo(User.GetUserName());
    }
}