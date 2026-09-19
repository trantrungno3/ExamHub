using TVT.Core.Claims;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using ExamHub.API.Authorization;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExamHub.Tests;

/// <summary>Fake ITeacherSubjectRepository — chỉ IsTeacherOfSubjectAsync được handler gọi.</summary>
file sealed class FakeTeacherSubjectRepository : ITeacherSubjectRepository
{
    public bool TeachesSubject { get; set; }

    public Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => Task.FromResult(TeachesSubject);

    public Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task SetSubjectsAsync(Guid userId, IReadOnlyList<int> subjectIds, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<TeacherSubject?> GetByIdAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<TeacherSubject>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<TeacherSubject>> GetAsync(Expression<Func<TeacherSubject, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<TeacherSubject?> FirstOrDefaultAsync(Expression<Func<TeacherSubject, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<TeacherSubject, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<TeacherSubject, bool>>? predicate = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<TeacherSubject> AddAsync(TeacherSubject entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<TeacherSubject> entities, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task UpdateAsync(TeacherSubject entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteAsync(TeacherSubject entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteByIdAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

/// <summary>
/// Ranh giới phân quyền của API. Không dựng host HTTP thật: bài test ở đây khoá đúng ba thứ dễ bị
/// phá âm thầm — endpoint nào được vào khi chưa đăng nhập, giáo viên có đúng môn phụ trách không,
/// và ai đọc được bài nộp của người khác.
/// </summary>
public class AuthorizationBoundaryTests
{
    /// <summary>
    /// CurrentUserInfo gọi GetUserid()/GetUserName()/GetDisplayName() của TVT.Core và mỗi hàm ném
    /// ArgumentException nếu thiếu claim — mọi identity trong file này phải có cả ba.
    /// Cùng ràng buộc đã ghi ở CurrentUserInfoTests và TeacherOwnsCohortClassHandlerTests.
    /// </summary>
    private static ClaimsPrincipal Identity(Guid userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new("UserId", userId.ToString()),
            new("UserName", "u1"),
            new("DisplayName", "User One"),
        };
        // Một loại claim duy nhất, đúng như production: token ghi role bằng ConstClaim.Role
        // (UserService.CreateTokenJwt) và TokenValidationParameters đặt RoleClaimType =
        // ConstClaim.Role (AuthExtension), nên IsInRole và CurrentUserInfo.GetRoles đọc CÙNG claim.
        // Phải truyền roleType cho ClaimsIdentity, vì mặc định nó là ClaimTypes.Role — dựng identity
        // theo mặc định là test IsInRole trên một claim mà production không bao giờ phát.
        claims.AddRange(roles.Select(r => new Claim(ConstClaim.Role, r)));
        return new ClaimsPrincipal(new ClaimsIdentity(
            claims, "test", ClaimTypes.Name, ConstClaim.Role));
    }

    // ── Chưa đăng nhập: fallback policy RequireAuthenticatedUser phủ toàn bộ API ──────────────

    [Fact]
    public void Only_login_register_and_refresh_are_reachable_without_authentication()
    {
        var anonymous = typeof(AuthorizeControllerBase).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")
            .OrderBy(x => x)
            .ToList();

        // Program.cs đặt SetFallbackPolicy(RequireAuthenticatedUser), nên mọi action KHÔNG có
        // [AllowAnonymous] đều trả 401 khi thiếu token. Danh sách dưới đây là toàn bộ ngoại lệ —
        // thêm một [AllowAnonymous] mới sẽ làm test này đỏ, đúng ý đồ.
        Assert.Equal(
            ["AuthController.Login", "AuthController.RefreshToken", "AuthController.Register"],
            anonymous);
    }

    // ── Giáo viên chỉ được thao tác trong môn mình phụ trách ─────────────────────────────────

    // Handler này tra DB (khác TeacherOwnsCohortClassHandler chỉ đọc claim), nên cần fake repo.
    private static async Task<bool> AuthorizeSubjectAsync(
        ClaimsPrincipal user, int subjectId, bool teachesSubject)
    {
        var handler = new TeacherOwnsSubjectHandler(
            new FakeTeacherSubjectRepository { TeachesSubject = teachesSubject });
        var context = new AuthorizationHandlerContext(
            [new TeacherOwnsSubjectRequirement()], user, subjectId);
        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Teacher_not_assigned_to_the_subject_is_denied()
    {
        var teacher = Identity(Guid.NewGuid(), "Teacher");

        Assert.False(await AuthorizeSubjectAsync(teacher, 9, teachesSubject: false));
    }

    [Fact]
    public async Task Teacher_assigned_to_the_subject_is_allowed()
    {
        var teacher = Identity(Guid.NewGuid(), "Teacher");

        Assert.True(await AuthorizeSubjectAsync(teacher, 9, teachesSubject: true));
    }

    [Fact]
    public async Task Admin_is_allowed_without_a_subject_assignment()
    {
        var admin = Identity(Guid.NewGuid(), "Admin");

        Assert.True(await AuthorizeSubjectAsync(admin, 9, teachesSubject: false));
    }

    [Fact]
    public async Task Student_is_denied_on_a_teacher_only_requirement()
    {
        var student = Identity(Guid.NewGuid(), "Student");

        Assert.False(await AuthorizeSubjectAsync(student, 9, teachesSubject: false));
    }

    // ── Đọc bài nộp: mặc định từ chối, chỉ chủ bài hoặc người chấm được xem ──────────────────

    [Fact]
    public void Student_cannot_read_another_students_submission()
    {
        var student = new CurrentUserInfo(Identity(Guid.NewGuid(), "Student"));

        Assert.False(SubmissionAccess.CanRead(student, ownerStudentId: Guid.NewGuid()));
    }

    [Fact]
    public void Student_can_read_their_own_submission()
    {
        var studentId = Guid.NewGuid();
        var student = new CurrentUserInfo(Identity(studentId, "Student"));

        Assert.True(SubmissionAccess.CanRead(student, ownerStudentId: studentId));
    }

    [Theory]
    [InlineData("Teacher")]
    [InlineData("Admin")]
    public void Graders_can_read_any_submission(string role)
    {
        var grader = new CurrentUserInfo(Identity(Guid.NewGuid(), role));

        Assert.True(SubmissionAccess.CanRead(grader, ownerStudentId: Guid.NewGuid()));
    }

    [Fact]
    public void A_user_with_no_role_cannot_read_someone_elses_submission()
    {
        var nobody = new CurrentUserInfo(Identity(Guid.NewGuid()));

        Assert.False(SubmissionAccess.CanRead(nobody, ownerStudentId: Guid.NewGuid()));
    }
}
