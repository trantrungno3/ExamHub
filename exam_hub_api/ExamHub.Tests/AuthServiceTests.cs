using System.Reflection;
using System.Security.Claims;
using ExamHub.API.Controllers;
using ExamHub.Core;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataAccessObjects;
using ExamHub.Core.DataTransferObjects.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TVT.Core.Claims;
using TVT.Core.Db.PostgreSql.Services;
using TVT.Core.Enums;
using TVT.Core.Extensions;
using TVT.Core.IdentityUser.PostgreSql.Models;
using TVT.Core.Models;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeTokenClaimsResolver : ITokenClaimsResolver
{
    public List<KeyValuePair<string, string>> ClaimsToReturn { get; set; } = [];
    public List<(Guid userId, IReadOnlyList<string> roles)> Calls { get; } = [];

    public Task<List<KeyValuePair<string, string>>> ResolveAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken ct = default)
    {
        Calls.Add((userId, roles));
        return Task.FromResult(ClaimsToReturn);
    }
}

file sealed class FakeUserServiceForLogin : IUserService
{
    public UserAdmin? UserToReturn { get; set; }
    public UserAdmin? CreatedUser { get; private set; }
    public UserAdmin? UpdatedUser { get; private set; }
    public List<KeyValuePair<string, string>>? CapturedCustomData { get; private set; }
    public string AccessTokenToReturn { get; set; } = "new-access-token";
    public string RefreshTokenToReturn { get; set; } = "new-refresh-token";

    public IEnumerable<UserAdmin> GetList() => throw new NotSupportedException();
    public Task<UserAdmin?> CreateAsync(UserAdmin user)
    {
        CreatedUser = user;
        return Task.FromResult<UserAdmin?>(user);
    }
    public Task<UserAdmin?> FindByIdAsync(Guid id) => throw new NotSupportedException();
    public Task<UserAdmin?> FindByNameAsync(string userName) => Task.FromResult(UserToReturn);
    public Task<UserAdmin?> FindByEmailAsync(string email) => throw new NotSupportedException();
    public Task<int> UpdateOneFieldAsync<TField>(Guid id, System.Linq.Expressions.Expression<Func<UserAdmin, TField>> field, TField value) => throw new NotSupportedException();
    public Task<int> UpdateAsync(UserAdmin user)
    {
        UpdatedUser = user;
        return Task.FromResult(1);
    }
    public Task<int> UpdateFieldsAsync(Guid id, params TVT.Core.Db.PostgreSql.SqlBuilder.FieldUpdate<UserAdmin>[] fields) => throw new NotSupportedException();
    public Task<bool> CheckUserNameExistAsync(string userName) => throw new NotSupportedException();
    public Task<bool> CheckUserExistByIdAsync(Guid id) => throw new NotSupportedException();
    public Task<int> DeleteAsync(UserAdmin user) => throw new NotSupportedException();

    public Task<(string, string)> CreateTokenJwt(ConfigAudience audienceConfig, ConfigAudience audienceRefresh, UserAdmin user, TimeSpan expireTime)
        => throw new NotSupportedException("Login phải dùng overload có customData");

    public Task<(string, string)> CreateTokenJwt(ConfigAudience audienceConfig, ConfigAudience audienceRefresh, UserAdmin user,
        List<KeyValuePair<string, string>> customData, TimeSpan expireTime)
    {
        CapturedCustomData = customData;
        return Task.FromResult((AccessTokenToReturn, RefreshTokenToReturn));
    }

    public string CreateTokenJwt(ConfigAudience audienceConfig, UserAdmin user, TimeSpan expireTime) => throw new NotSupportedException();
}

public class AuditUserWriteTests
{
    static AuditUserWriteTests() => AppCommon.SaltPassHash = "test-salt";

    [Fact]
    public async Task Register_stamps_creator_and_modifier()
    {
        var users = new FakeUserServiceForLogin();
        var service = new AuthService(users, new FakeTokenClaimsResolver());

        await service.Register(new RegisterDto
        {
            UserName = "student1",
            Password = "secret1",
            DisplayName = "Student One",
        });

        Assert.Equal("student1", users.CreatedUser!.CreatedBy);
        Assert.Equal("student1", users.CreatedUser.ModifiedBy);
        Assert.NotNull(users.CreatedUser.Modified);
    }

    [Fact]
    public async Task UpdateProfile_stamps_current_user()
    {
        var user = User("student1");
        var users = new FakeUserServiceForLogin { UserToReturn = user };
        var service = new AuthService(users, new FakeTokenClaimsResolver());

        await service.UpdateProfile("student1", new UpdateProfileDto
        {
            DisplayName = "Student One Updated",
        });

        Assert.Same(user, users.UpdatedUser);
        Assert.Equal("student1", user.ModifiedBy);
        Assert.NotNull(user.Modified);
    }

    [Fact]
    public async Task ChangePassword_stamps_current_user()
    {
        var user = User("student1");
        user.PasswordHash = "old-secret".GetPasswordHash(AppCommon.SaltPassHash!);
        var users = new FakeUserServiceForLogin { UserToReturn = user };
        var service = new AuthService(users, new FakeTokenClaimsResolver());

        await service.ChangePassword("student1", new ChangePasswordDto
        {
            OldPassword = "old-secret",
            NewPassword = "new-secret",
        });

        Assert.Same(user, users.UpdatedUser);
        Assert.Equal("student1", user.ModifiedBy);
        Assert.NotNull(user.Modified);
    }

    [Fact]
    public async Task AdminCreate_stamps_current_admin()
    {
        var users = new FakeUserServiceForLogin();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ConstClaim.UserName, "admin1")], "test"));
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };
        var service = new UserManagementService(users, accessor);

        await service.CreateAsync(new CreateUserRequest
        {
            UserName = "student1",
            Password = "secret1",
            DisplayName = "Student One",
        });

        Assert.Equal("admin1", users.CreatedUser!.CreatedBy);
        Assert.Equal("admin1", users.CreatedUser.ModifiedBy);
        Assert.NotNull(users.CreatedUser.Modified);
    }

    private static UserAdmin User(string userName) => new()
    {
        Id = Guid.NewGuid(),
        UserName = userName,
        DisplayName = "Student One",
    };
}

public class AuthServiceLoginClaimsTests
{
    // Không có test nào khác trong ExamHub.Tests set AppCommon.SaltPassHash (đã kiểm tra —
    // grep "SaltPassHash" trong exam_hub_api/ExamHub.Tests không có kết quả), nên set ở đây
    // để AuthService.Login không NullReferenceException khi gọi GetPasswordHash(AppCommon.SaltPassHash!).
    static AuthServiceLoginClaimsTests() => AppCommon.SaltPassHash = "test-salt";

    [Fact]
    public async Task Login_PassesResolvedClaimsIntoCreateTokenJwt()
    {
        // UserAdmin.Roles has a private setter (set only via AddRole) — must call it
        // after construction rather than in the object initializer.
        var user = new UserAdmin
        {
            Id = Guid.NewGuid(),
            UserName = "teacher1",
            DisplayName = "Teacher One",
            PasswordHash = "irrelevant".GetPasswordHash(AppCommon.SaltPassHash!),
        };
        user.AddRole("Teacher");
        var userService = new FakeUserServiceForLogin { UserToReturn = user };
        var resolver = new FakeTokenClaimsResolver
        {
            ClaimsToReturn = [new("SchoolId", "5"), new("CohortClassId", "100")],
        };
        var authService = new AuthService(userService, resolver);

        await authService.Login(new LoginDto { UserName = "teacher1", Password = "irrelevant" });

        Assert.Equal(userService.UserToReturn.Id, resolver.Calls[0].userId);
        Assert.Equal(["Teacher"], resolver.Calls[0].roles);
        Assert.Equal(resolver.ClaimsToReturn, userService.CapturedCustomData);
    }
}

public class AuthControllerRefreshContractTests
{
    [Fact]
    public void RefreshToken_UsesPostBodyAndReturnsTokenModel()
    {
        var action = typeof(AuthController).GetMethod(nameof(AuthController.RefreshToken))!;

        var post = Assert.Single(action.GetCustomAttributes<HttpPostAttribute>());
        Assert.Equal("refresh-token", post.Template);
        Assert.Empty(action.GetCustomAttributes<HttpGetAttribute>());

        var parameter = Assert.Single(action.GetParameters());
        Assert.NotNull(parameter.GetCustomAttribute<FromBodyAttribute>());

        // Verify return type is Task<ActionResult<RequestResponse<TokenModel>>>
        Assert.True(action.ReturnType.IsGenericType);
        Assert.Equal("Task`1", action.ReturnType.Name);
        var actionResultType = action.ReturnType.GetGenericArguments()[0];
        Assert.Equal("ActionResult`1", actionResultType.Name);
        var responseType = actionResultType.GetGenericArguments()[0];
        Assert.Equal("RequestResponse`1", responseType.Name);
        Assert.Equal("TokenModel", responseType.GetGenericArguments()[0].Name);
    }
}

public class AuthServiceRefreshTests
{
    private static void ConfigureTestAudiences()
    {
        var values = new Dictionary<string, string?>
        {
            ["Audience:Aud"] = "exam-hub-test",
            ["Audience:Iss"] = "exam-hub-test",
            ["Audience:Secret"] = "exam-hub-test-access-secret-32-bytes",
            ["Refresh:Aud"] = "exam-hub-refresh-test",
            ["Refresh:Iss"] = "exam-hub-refresh-test",
            ["Refresh:Secret"] = "exam-hub-test-refresh-secret-32-bytes",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        configuration.GetSection("Audience").Bind(AppCommon.Audience);
        configuration.GetSection("Refresh").Bind(AppCommon.AudienceRefresh);
    }

    [Fact]
    public async Task RefreshToken_ValidPair_ReturnsRotatedPairWithResolvedClaims()
    {
        ConfigureTestAudiences();
        var user = new UserAdmin
        {
            Id = Guid.NewGuid(),
            UserName = "teacher1",
            DisplayName = "Teacher One",
        };
        user.AddRole("Teacher");

        var accessToken = AppCommon.Audience.EncodedJwt(
            [new Claim("UserName", user.UserName)],
            TimeSpan.FromMinutes(5));
        var refreshToken = AppCommon.AudienceRefresh.EncodedJwt(
            [new Claim("UserName", user.UserName)],
            TimeSpan.FromMinutes(30));
        user.RefreshToken = refreshToken;

        var userService = new FakeUserServiceForLogin
        {
            UserToReturn = user,
            AccessTokenToReturn = "rotated-access-token",
            RefreshTokenToReturn = "rotated-refresh-token",
        };
        var resolver = new FakeTokenClaimsResolver
        {
            ClaimsToReturn = [new("SchoolId", "5"), new("SubjectId", "9")],
        };
        var service = new AuthService(userService, resolver);

        var result = await service.RefreshToken(accessToken, refreshToken);

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal("rotated-access-token", result.Data.AccessToken);
        Assert.Equal("rotated-refresh-token", result.Data.RefreshToken);
        Assert.Equal(resolver.ClaimsToReturn, userService.CapturedCustomData);
        Assert.Single(resolver.Calls);
    }

    [Fact]
    public async Task RefreshToken_MissingRefreshToken_ReturnsErrorWithoutTouchingUserStore()
    {
        ConfigureTestAudiences();
        var userService = new FakeUserServiceForLogin();
        var service = new AuthService(userService, new FakeTokenClaimsResolver());

        var result = await service.RefreshToken("some-access-token", "");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Null(userService.UpdatedUser);
    }
}

public class AuthServiceRevokeTests
{
    static AuthServiceRevokeTests() => AppCommon.SaltPassHash = "test-salt";

    private static UserAdmin UserWithRefreshToken(string userName = "teacher1") => new()
    {
        Id = Guid.NewGuid(),
        UserName = userName,
        DisplayName = "Teacher One",
        RefreshToken = "stored-refresh-token",
    };

    [Fact]
    public async Task RevokeRefreshToken_ClearsStoredTokenAndPersists()
    {
        var user = UserWithRefreshToken();
        var userService = new FakeUserServiceForLogin { UserToReturn = user };
        var service = new AuthService(userService, new FakeTokenClaimsResolver());

        var result = await service.RevokeRefreshToken(user.UserName!);

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Same(user, userService.UpdatedUser);
        Assert.Null(userService.UpdatedUser!.RefreshToken);
    }

    [Fact]
    public async Task RevokeRefreshToken_UnknownUser_ReturnsErrorWithoutUpdate()
    {
        var userService = new FakeUserServiceForLogin { UserToReturn = null };
        var service = new AuthService(userService, new FakeTokenClaimsResolver());

        var result = await service.RevokeRefreshToken("ghost");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Null(userService.UpdatedUser);
    }

    [Fact]
    public async Task ChangePassword_RevokesRefreshTokenSoStolenCookieCannotRefresh()
    {
        var user = UserWithRefreshToken();
        user.PasswordHash = "old-pass".GetPasswordHash(AppCommon.SaltPassHash!);
        var userService = new FakeUserServiceForLogin { UserToReturn = user };
        var service = new AuthService(userService, new FakeTokenClaimsResolver());

        var result = await service.ChangePassword(
            user.UserName!,
            new ChangePasswordDto { OldPassword = "old-pass", NewPassword = "new-pass-123" });

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Null(userService.UpdatedUser!.RefreshToken);
    }
}
