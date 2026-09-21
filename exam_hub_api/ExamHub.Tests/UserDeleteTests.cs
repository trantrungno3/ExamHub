using System.Linq.Expressions;
using ExamHub.API.Controllers;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.User;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVT.Core;
using TVT.Core.IdentityUser.PostgreSql.Models;
using Xunit;

namespace ExamHub.Tests;

// ── Hand-rolled fakes (no mocking library in this repo — xunit only) ──────
// D5 puts the "user has related submissions" guard directly in UserController
// (per the task-D5 ruling: IUserManagementService has no DeleteAsync(id, force)
// overload, and User keeps the ?force=true query param instead of a /force
// sub-route since UserController is not a CategoryBaseController child). That
// means these tests must exercise the controller action itself.

/// <summary>Fake IExamSubmissionRepository backed by an in-memory list, with a call log
/// so tests can assert whether force-delete actually cleaned up submissions.</summary>
file sealed class FakeExamSubmissionRepository(List<string> callLog) : IExamSubmissionRepository
{
    public List<ExamSubmission> Submissions { get; } = [];

    public Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ExamSubmission>>(Submissions.Where(s => s.StudentId == studentId).ToList());

    public Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
        => operation(ct);

    public Task<ExamSubmission?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<ExamSubmission?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<(IReadOnlyList<ExamSubmission> Items, int Total)> GetPageBySessionAsync(Guid sessionId, int page, int pageSize, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<(IReadOnlyList<ExamSubmission> Items, int Total)> GetPageByStudentAsync(Guid studentId, int page, int pageSize, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamSubmission>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<ExamSubmission>> GetBySessionAndStudentAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyDictionary<Guid, string>> GetStudentClassNamesAsync(IReadOnlyCollection<Guid> studentIds, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<ExamSubmission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<ExamSubmission>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<ExamSubmission>> GetAsync(Expression<Func<ExamSubmission, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<ExamSubmission?> FirstOrDefaultAsync(Expression<Func<ExamSubmission, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsAsync(Expression<Func<ExamSubmission, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<int> CountAsync(Expression<Func<ExamSubmission, bool>>? predicate = null, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<ExamSubmission> AddAsync(ExamSubmission entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task AddRangeAsync(IEnumerable<ExamSubmission> entities, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task UpdateAsync(ExamSubmission entity, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task DeleteAsync(ExamSubmission entity, CancellationToken ct = default)
    {
        Submissions.RemoveAll(s => s.Id == entity.Id);
        callLog.Add($"submission-delete:{entity.Id}");
        return Task.CompletedTask;
    }

    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

/// <summary>Fake IUserManagementService — only FindByIdAsync/DeleteAsync are exercised
/// by UserController.Delete. Records DeleteAsync calls for assertions. Set
/// <see cref="ThrowDbUpdateExceptionOnDelete"/> to simulate an FK violation that isn't
/// caught by the student-submission guard (e.g. the user is referenced via
/// submission_answers.graded_by or questions.verified_by, neither of which this
/// controller queries directly).</summary>
file sealed class FakeUserManagementService(List<string> callLog) : IUserManagementService
{
    public List<UserAdmin> Users { get; } = [];
    public List<Guid> DeleteCalls { get; } = [];
    public bool ThrowDbUpdateExceptionOnDelete { get; set; }

    public IEnumerable<UserAdmin> GetList() => throw new NotSupportedException();

    public Task<UserAdmin?> FindByIdAsync(Guid id)
        => Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

    public Task<bool> CheckUserNameExistAsync(string userName) => throw new NotSupportedException();

    public Task<bool> CheckUserExistByIdAsync(Guid id) => throw new NotSupportedException();

    public Task<UserAdmin?> CreateAsync(CreateUserRequest request) => throw new NotSupportedException();

    public Task<UserAdmin> UpdateAsync(UserAdmin user, UpdateUserRequest request) => throw new NotSupportedException();

    public Task DeleteAsync(UserAdmin user)
    {
        if (ThrowDbUpdateExceptionOnDelete)
        {
            callLog.Add($"user-delete-attempt-fk-violation:{user.Id}");
            throw new DbUpdateException("FK violation: user referenced by submission_answers.graded_by or questions.verified_by");
        }

        DeleteCalls.Add(user.Id);
        Users.RemoveAll(u => u.Id == user.Id);
        callLog.Add($"user-delete:{user.Id}");
        return Task.CompletedTask;
    }

    public Task SetLockAsync(Guid id, bool isLocked) => throw new NotSupportedException();

    public Task ResetPasswordAsync(Guid id, string newPassword) => throw new NotSupportedException();

    public Task SetRolesAsync(Guid id, string[] roles) => throw new NotSupportedException();

    public Task<string[]?> AddRoleAsync(UserAdmin user, string role) => throw new NotSupportedException();

    public Task<string[]?> RemoveRoleAsync(UserAdmin user, string role) => throw new NotSupportedException();
}

/// <summary>Fake IUserBulkImportService — unused by the Delete action, only needed
/// to satisfy the UserController constructor.</summary>
file sealed class FakeUserBulkImportService : IUserBulkImportService
{
    public Task<BulkUserImportResponse> ImportAsync(BulkUserImportRequest request, CancellationToken ct = default)
        => throw new NotSupportedException();

    public byte[] BuildTemplate() => throw new NotSupportedException();
}

// ── Tests ───────────────────────────────────────────────────────────────

public class UserControllerDeleteTests
{
    private static UserAdmin NewUser(Guid id) => new()
    {
        Id = id,
        UserName = "u" + id,
        DisplayName = "User " + id,
    };

    private static ExamSubmission NewSubmission(Guid studentId) => new()
    {
        Id = Guid.NewGuid(),
        ExamId = Guid.NewGuid(),
        StudentId = studentId,
    };

    [Fact]
    public async Task Delete_NoForce_WithSubmissions_ReturnsConflict_AndDoesNotDeleteUser()
    {
        var log = new List<string>();
        var userService = new FakeUserManagementService(log);
        var submissionRepo = new FakeExamSubmissionRepository(log);
        var controller = new UserController(userService, new FakeUserBulkImportService(), submissionRepo);
        var userId = Guid.NewGuid();
        userService.Users.Add(NewUser(userId));
        submissionRepo.Submissions.Add(NewSubmission(userId));

        var result = await controller.Delete(userId, force: false, ct: default);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        var response = Assert.IsType<RequestResponse<object>>(conflict.Value);
        Assert.Equal(
            "Người dùng đang có dữ liệu liên quan (bài làm/đề thi). Dùng xoá bắt buộc nếu chắc chắn.",
            response.Message);
        Assert.Single(userService.Users); // user untouched
        Assert.Empty(userService.DeleteCalls);
        Assert.Single(submissionRepo.Submissions); // submission untouched
        Assert.Empty(log);
    }

    [Fact]
    public async Task Delete_NoForce_NoSubmissions_DeletesUser_ReturnsNoContent()
    {
        var log = new List<string>();
        var userService = new FakeUserManagementService(log);
        var submissionRepo = new FakeExamSubmissionRepository(log);
        var controller = new UserController(userService, new FakeUserBulkImportService(), submissionRepo);
        var userId = Guid.NewGuid();
        userService.Users.Add(NewUser(userId));

        var result = await controller.Delete(userId, force: false, ct: default);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(userService.Users);
        Assert.Equal([userId], userService.DeleteCalls);
        Assert.Equal([$"user-delete:{userId}"], log);
    }

    [Fact]
    public async Task Delete_Force_WithSubmissions_DeletesSubmissionsThenUser_ReturnsNoContent()
    {
        var log = new List<string>();
        var userService = new FakeUserManagementService(log);
        var submissionRepo = new FakeExamSubmissionRepository(log);
        var controller = new UserController(userService, new FakeUserBulkImportService(), submissionRepo);
        var userId = Guid.NewGuid();
        userService.Users.Add(NewUser(userId));
        var s1 = NewSubmission(userId);
        var s2 = NewSubmission(userId);
        submissionRepo.Submissions.Add(s1);
        submissionRepo.Submissions.Add(s2);

        var result = await controller.Delete(userId, force: true, ct: default);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(userService.Users);
        Assert.Empty(submissionRepo.Submissions); // cleanup actually ran
        Assert.Equal(
            [$"submission-delete:{s1.Id}", $"submission-delete:{s2.Id}", $"user-delete:{userId}"],
            log);
    }

    [Fact]
    public async Task Delete_Force_NoStudentSubmissions_ButFkViolationOnDelete_ReturnsConflict_NotUnhandledException()
    {
        // Covers the review finding: the student-submission guard only checks
        // GetByStudentAsync, so it stays silent when the user is referenced elsewhere
        // (submission_answers.graded_by, questions.verified_by, ...). Force-delete must
        // still surface a Vietnamese 409 instead of an unhandled DbUpdateException/500.
        var log = new List<string>();
        var userService = new FakeUserManagementService(log) { ThrowDbUpdateExceptionOnDelete = true };
        var submissionRepo = new FakeExamSubmissionRepository(log);
        var controller = new UserController(userService, new FakeUserBulkImportService(), submissionRepo);
        var userId = Guid.NewGuid();
        userService.Users.Add(NewUser(userId));
        // No student submissions on record → the existing guard would not block this.

        var result = await controller.Delete(userId, force: true, ct: default);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        var response = Assert.IsType<RequestResponse<object>>(conflict.Value);
        Assert.Equal(
            "Không thể xoá người dùng do còn dữ liệu liên quan (bài chấm, câu hỏi đã duyệt, ...).",
            response.Message);
        Assert.Single(userService.Users); // user untouched — delete did not silently "succeed"
        Assert.Empty(userService.DeleteCalls);
    }

    [Fact]
    public async Task Delete_UserNotFound_ReturnsNotFound()
    {
        var log = new List<string>();
        var userService = new FakeUserManagementService(log);
        var submissionRepo = new FakeExamSubmissionRepository(log);
        var controller = new UserController(userService, new FakeUserBulkImportService(), submissionRepo);

        var result = await controller.Delete(Guid.NewGuid(), force: false, ct: default);

        Assert.IsType<NotFoundResult>(result);
    }
}
