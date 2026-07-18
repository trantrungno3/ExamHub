using ExamHub.Core.DataAccessObjects;
using TVT.Core;
using TVT.Core.IdentityUser.PostgreSql.Models;
using TVT.Core.Models;

namespace ExamHub.Core.Application.Services;

public interface IAuthService
{
    /// <summary>
    ///     Đăng nhập
    /// </summary>
    /// <param name="dto">Thông tin đăng nhập</param>
    /// <returns></returns>
    Task<RequestResponse<TokenModel>> Login(LoginDto dto);

    /// <summary>
    ///     Đăng ký tài khoản
    /// </summary>
    /// <param name="dto">Thông tin đăng ký</param>
    /// <returns></returns>
    Task<RequestResponse<object>> Register(RegisterDto dto);

    /// <summary>
    ///     Lấy token mới bằng refesh token
    /// </summary>
    /// <param name="dto">Thông tin token</param>
    /// <returns></returns>
    Task<RequestResponse<string>> RefreshToken(TokenModel dto);

    /// <summary>
    ///     Lấy thông tin người dùng
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<RequestResponse<UserInfo>> GetUserInfo(string userName);

    /// <summary>
    ///     Cập nhật thông tin cá nhân của người dùng đang đăng nhập
    /// </summary>
    Task<RequestResponse<UserInfo>> UpdateProfile(string userName, UpdateProfileDto dto);

    /// <summary>
    ///     Đổi mật khẩu của người dùng đang đăng nhập
    /// </summary>
    Task<RequestResponse<bool>> ChangePassword(string userName, ChangePasswordDto dto);
}