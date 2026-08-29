using ExamHub.Core;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataAccessObjects;
using TVT.Core.Db.PostgreSql.Services;
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
    public List<KeyValuePair<string, string>>? CapturedCustomData { get; private set; }

    public IEnumerable<UserAdmin> GetList() => throw new NotSupportedException();
    public Task<UserAdmin?> CreateAsync(UserAdmin user) => throw new NotSupportedException();
    public Task<UserAdmin?> FindByIdAsync(Guid id) => throw new NotSupportedException();
    public Task<UserAdmin?> FindByNameAsync(string userName) => Task.FromResult(UserToReturn);
    public Task<UserAdmin?> FindByEmailAsync(string email) => throw new NotSupportedException();
    public Task<int> UpdateOneFieldAsync<TField>(Guid id, System.Linq.Expressions.Expression<Func<UserAdmin, TField>> field, TField value) => throw new NotSupportedException();
    public Task<int> UpdateAsync(UserAdmin user) => throw new NotSupportedException();
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
        return Task.FromResult(("fake-access-token", "fake-refresh-token"));
    }

    public string CreateTokenJwt(ConfigAudience audienceConfig, UserAdmin user, TimeSpan expireTime) => throw new NotSupportedException();
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
