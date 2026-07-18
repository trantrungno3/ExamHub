using ExamHub.Core.DataAccessObjects;
using TVT.Core;
using TVT.Core.Db.PostgreSql.Services;
using TVT.Core.Extensions;
using TVT.Core.IdentityUser.PostgreSql.Models;
using TVT.Core.Models;

namespace ExamHub.Core.Application.Services;

public sealed class AuthService(IUserService userService) : IAuthService
{
    /// <summary>
    ///     Đăng nhập
    /// </summary>
    /// <param name="dto">Thông tin đăng nhập</param>
    /// <returns></returns>
    public async Task<RequestResponse<TokenModel>> Login(LoginDto dto)
    {
        if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Password))
            return RequestResponse<TokenModel>.Error("Không được để trống thông tin!");
        var userInfo = await userService.FindByNameAsync(dto.UserName);
        if (userInfo == null)
            return RequestResponse<TokenModel>.Error("Không tìm thấy người dùng!");
        if (!userInfo.PasswordHash!.Contains(dto.Password.GetPasswordHash(AppCommon.SaltPassHash!)))
            return RequestResponse<TokenModel>.Error("Tài khoản hoặc mật khẩu sai!");
        var jwtToken = await userService.CreateTokenJwt(AppCommon.Audience, AppCommon.AudienceRefresh, userInfo,
            TimeSpan.FromHours(8));
        return RequestResponse<TokenModel>.Success("Đăng nhập thành công!",
            new TokenModel(jwtToken.Item1, jwtToken.Item2), 1);
    }

    /// <summary>
    ///     Đăng ký tài khoản
    /// </summary>
    /// <param name="dto">Thông tin đăng ký</param>
    /// <returns></returns>
    public async Task<RequestResponse<object>> Register(RegisterDto dto)
    {
        if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Password))
            return RequestResponse<object>.Error("Không được để trống thông tin!");
        var userInfo = await userService.FindByNameAsync(dto.UserName);
        if (userInfo != null)
            return RequestResponse<object>.Error("Người dùng đã tồn tại!");
        var data = dto.ToDomain();
        data.PasswordHash = dto.Password.GetPasswordHash(AppCommon.SaltPassHash!);
        var userAdmin = await userService.CreateAsync(data);
        return userAdmin != null
            ? RequestResponse<object>.Success("Đăng kí thành công!")
            : RequestResponse<object>.Error("Đăng kí thất bại!");
    }

    /// <summary>
    ///     Lấy token mới bằng refesh token
    /// </summary>
    /// <param name="dto">Thông tin token</param>
    /// <returns></returns>
    public async Task<RequestResponse<string>> RefreshToken(TokenModel dto)
    {
        if (string.IsNullOrEmpty(dto.AccessToken) || string.IsNullOrEmpty(dto.RefreshToken))
            return RequestResponse<string>.Error("Không được để trống thông tin!");
        var claims = AuthExtension.GetPrincipalFromExpiredToken(dto.AccessToken, AppCommon.Audience);
        if (claims == null)
            return RequestResponse<string>.Error("Token không được để trống!");
        var userName = claims.GetUserName();
        var userInfo = await userService.FindByNameAsync(userName!);
        if (userInfo == null || userInfo.RefreshToken != dto.RefreshToken)
            return RequestResponse<string>.Error("Token không hợp lệ!");

        var isRefreshValid = AuthExtension.ValidRefreshToken(dto.RefreshToken, AppCommon.AudienceRefresh);
        return isRefreshValid
            ? RequestResponse<string>.Success("Lấy token thành công!",
                userService.CreateTokenJwt(AppCommon.Audience, userInfo, TimeSpan.FromHours(8)), 1)
            : RequestResponse<string>.Error("Token đã hết hạn!");
    }

    /// <summary>
    ///     Lấy thông tin tài khoản
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<RequestResponse<UserInfo>> GetUserInfo(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return RequestResponse<UserInfo>.Error("Không được để trống thông tin!");
        var userInfo = await userService.FindByNameAsync(userName);
        return userInfo == null
            ? RequestResponse<UserInfo>.Error("Không tìm thấy thông tin!")
            : RequestResponse<UserInfo>.Success("Lấy token thành công!", new UserInfo(userInfo), 1);
    }

    /// <summary>
    ///     Cập nhật thông tin cá nhân của người dùng đang đăng nhập
    /// </summary>
    public async Task<RequestResponse<UserInfo>> UpdateProfile(string userName, UpdateProfileDto dto)
    {
        if (string.IsNullOrEmpty(userName))
            return RequestResponse<UserInfo>.Error("Không xác định được người dùng!");
        var user = await userService.FindByNameAsync(userName);
        if (user == null)
            return RequestResponse<UserInfo>.Error("Không tìm thấy thông tin!");

        user.DisplayName = dto.DisplayName;
        user.PhoneNumber = dto.PhoneNumber;
        if (!string.IsNullOrEmpty(dto.Email))
            user.SetEmail(dto.Email);
        user.Modified = DateTime.UtcNow;
        await userService.UpdateAsync(user);

        return RequestResponse<UserInfo>.Success("Cập nhật thông tin thành công!", new UserInfo(user), 1);
    }

    /// <summary>
    ///     Đổi mật khẩu của người dùng đang đăng nhập
    /// </summary>
    public async Task<RequestResponse<bool>> ChangePassword(string userName, ChangePasswordDto dto)
    {
        if (string.IsNullOrEmpty(userName))
            return RequestResponse<bool>.Error("Không xác định được người dùng!");
        if (string.IsNullOrEmpty(dto.OldPassword) || string.IsNullOrEmpty(dto.NewPassword))
            return RequestResponse<bool>.Error("Không được để trống thông tin!");

        var user = await userService.FindByNameAsync(userName);
        if (user == null)
            return RequestResponse<bool>.Error("Không tìm thấy thông tin!");

        if (!user.PasswordHash!.Contains(dto.OldPassword.GetPasswordHash(AppCommon.SaltPassHash!)))
            return RequestResponse<bool>.Error("Mật khẩu hiện tại không đúng!");

        user.PasswordHash = dto.NewPassword.GetPasswordHash(AppCommon.SaltPassHash!);
        user.Modified = DateTime.UtcNow;
        await userService.UpdateAsync(user);

        return RequestResponse<bool>.Success("Đổi mật khẩu thành công!", true, 1);
    }
}