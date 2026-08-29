using System.Security.Claims;
using ExamHub.API.Authorization;
using ExamHub.Core.Application.Services;
using Xunit;

namespace ExamHub.Tests;

public class CurrentUserInfoTests
{
    // CurrentUserInfo's ctor calls TVT.Core's user.GetUserid()/GetUserName()/GetDisplayName()
    // unconditionally, and each throws ArgumentException if its claim is missing — every
    // non-null test needs all three present even though these tests don't assert on them.
    private static ClaimsIdentity BaseIdentity() => new(
    [
        new Claim("UserId", Guid.NewGuid().ToString()),
        new Claim("UserName", "u1"),
        new Claim("DisplayName", "User One"),
    ]);

    [Fact]
    public void Constructor_ParsesMultiValuedSchoolClassSubjectClaims()
    {
        var identity = BaseIdentity();
        identity.AddClaims(
        [
            new Claim(TokenClaimTypes.SchoolId, "5"),
            new Claim(TokenClaimTypes.SchoolId, "6"),
            new Claim(TokenClaimTypes.CohortClassId, "100"),
            new Claim(TokenClaimTypes.SubjectId, "7"),
        ]);
        var principal = new ClaimsPrincipal(identity);

        var info = new CurrentUserInfo(principal);

        Assert.Equal([5, 6], info.SchoolIds);
        Assert.Equal([100], info.CohortClassIds);
        Assert.Equal([7], info.SubjectIds);
    }

    [Fact]
    public void Constructor_NoClaims_ReturnsEmptyLists()
    {
        var principal = new ClaimsPrincipal(BaseIdentity());

        var info = new CurrentUserInfo(principal);

        Assert.Empty(info.SchoolIds);
        Assert.Empty(info.CohortClassIds);
        Assert.Empty(info.SubjectIds);
    }

    [Fact]
    public void Constructor_NullPrincipal_ReturnsEmptyLists()
    {
        var info = new CurrentUserInfo(null);

        Assert.Empty(info.SchoolIds);
    }
}
