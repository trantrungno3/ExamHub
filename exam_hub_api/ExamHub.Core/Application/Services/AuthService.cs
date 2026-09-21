using ExamHub.Core.DataAccessObjects;
using TVT.Core;
using TVT.Core.Db.PostgreSql.Services;
using TVT.Core.Extensions;
using TVT.Core.IdentityUser.PostgreSql.Models;
using TVT.Core.Models;

namespace ExamHub.Core.Application.Services;

public sealed class AuthService(IUserService userService, ITokenClaimsResolver tokenClaimsResolver) : IAuthService
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
        var customClaims = await tokenClaimsResolver.ResolveAsync(userInfo.Id, userInfo.Roles);
        var jwtToken = await userService.CreateTokenJwt(AppCommon.Audience, AppCommon.AudienceRefresh, userInfo,
            customClaims, TimeSpan.FromHours(8));
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
        var now = DateTime.UtcNow;
        data.PasswordHash = dto.Password.GetPasswordHash(AppCommon.SaltPassHash!);
        data.Created = now;
        data.CreatedBy = dto.UserName;
        data.Modified = now;
        data.ModifiedBy = dto.UserName;
        var userAdmin = await userService.CreateAsync(data);
        return userAdmin != null
            ? RequestResponse<object>.Success("Đăng kí thành công!")
            : RequestResponse<object>.Error("Đăng kí thất bại!");
    }

    /// <summary>
    ///     Lấy token mới bằng refresh token
    /// </summary>
    /// <param name="accessToken">Access token đã/sắp hết hạn</param>
    /// <param name="refreshToken">Refresh token đọc từ cookie HttpOnly</param>
    /// <returns></returns>
    public async Task<RequestResponse<TokenModel>> RefreshToken(string accessToken, string refreshToken)
    {
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            return RequestResponse<TokenModel>.Error("Không được để trống thông tin!");

        var claims = AuthExtension.GetPrincipalFromExpiredToken(accessToken, AppCommon.Audience);
        if (claims == null)
            return RequestResponse<TokenModel>.Error("Token không hợp lệ!");

        var userName = claims.GetUserName();
        var userInfo = await userService.FindByNameAsync(userName);
        if (userInfo == null || userInfo.RefreshToken != refreshToken)
            return RequestResponse<TokenModel>.Error("Token không hợp lệ!");

        if (!AuthExtension.ValidRefreshToken(refreshToken, AppCommon.AudienceRefresh))
            return RequestResponse<TokenModel>.Error("Token đã hết hạn!");

        var customClaims = await tokenClaimsResolver.ResolveAsync(userInfo.Id, userInfo.Roles);
        var tokens = await userService.CreateTokenJwt(
            AppCommon.Audience,
            AppCommon.AudienceRefresh,
            userInfo,
            customClaims,
            TimeSpan.FromHours(8));

        return RequestResponse<TokenModel>.Success(
            "Lấy token thành công!",
            new TokenModel(tokens.Item1, tokens.Item2),
            1);
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
        user.ModifiedBy = userName;
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
        // Đổi mật khẩu phải chặn luôn refresh token cũ: nếu không, cookie bị đánh cắp vẫn xin được
        // access token mới vô hạn dù mật khẩu đã đổi.
        user.RefreshToken = null;
        user.ModifiedBy = userName;
        user.Modified = DateTime.UtcNow;
        await userService.UpdateAsync(user);

        return RequestResponse<bool>.Success("Đổi mật khẩu thành công!", true, 1);
    }

    /// <summary>
    ///     Thu hồi refresh token đang lưu của người dùng (logout / đổi mật khẩu)
    /// </summary>
    public async Task<RequestResponse<bool>> RevokeRefreshToken(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return RequestResponse<bool>.Error("Không xác định được người dùng!");

        var user = await userService.FindByNameAsync(userName);
        if (user == null)
            return RequestResponse<bool>.Error("Không tìm thấy thông tin!");

        user.RefreshToken = null;
        user.ModifiedBy = userName;
        user.Modified = DateTime.UtcNow;
        await userService.UpdateAsync(user);

        return RequestResponse<bool>.Success("Đăng xuất thành công!", true, 1);
    }
}
