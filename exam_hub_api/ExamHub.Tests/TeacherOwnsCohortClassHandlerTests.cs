using TVT.Core.Claims;
using System.Security.Claims;
using ExamHub.API.Authorization;
using ExamHub.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace ExamHub.Tests;

public class TeacherOwnsCohortClassHandlerTests
{
    // CurrentUserInfo's ctor calls TVT.Core's user.GetUserid()/GetUserName()/GetDisplayName()
    // unconditionally inside the handler's CurrentUserInfo construction, and each throws
    // ArgumentException if its claim is missing — every identity below needs all three,
    // same requirement documented in CurrentUserInfoTests.
    // roleType phải là ConstClaim.Role để khớp RoleClaimType của TokenValidationParameters; dùng
    // mặc định (ClaimTypes.Role) là kiểm IsInRole trên claim mà token thật không phát.
    private static ClaimsIdentity BaseIdentity(string role) => new(
    [
        new Claim("UserId", Guid.NewGuid().ToString()),
        new Claim("UserName", "u1"),
        new Claim("DisplayName", "User One"),
        new Claim(ConstClaim.Role, role),
    ], "test", ClaimTypes.Name, ConstClaim.Role);

    private static async Task<bool> AuthorizeAsync(ClaimsPrincipal user, int cohortClassId)
    {
        var handler = new TeacherOwnsCohortClassHandler();
        var context = new AuthorizationHandlerContext(
            [new TeacherOwnsCohortClassRequirement()], user, cohortClassId);
        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Admin_AlwaysSucceeds_EvenWithoutClaim()
    {
        var user = new ClaimsPrincipal(BaseIdentity("Admin"));

        Assert.True(await AuthorizeAsync(user, 100));
    }

    [Fact]
    public async Task Teacher_WithMatchingCohortClassIdClaim_Succeeds()
    {
        var identity = BaseIdentity("Teacher");
        identity.AddClaim(new Claim(TokenClaimTypes.CohortClassId, "100"));
        var user = new ClaimsPrincipal(identity);

        Assert.True(await AuthorizeAsync(user, 100));
    }

    [Fact]
    public async Task Teacher_WithDifferentCohortClassIdClaim_Fails()
    {
        var identity = BaseIdentity("Teacher");
        identity.AddClaim(new Claim(TokenClaimTypes.CohortClassId, "200"));
        var user = new ClaimsPrincipal(identity);

        Assert.False(await AuthorizeAsync(user, 100));
    }

    [Fact]
    public async Task Teacher_NoCohortClassIdClaimsAtAll_Fails()
    {
        var user = new ClaimsPrincipal(BaseIdentity("Teacher"));

        Assert.False(await AuthorizeAsync(user, 100));
    }
}
