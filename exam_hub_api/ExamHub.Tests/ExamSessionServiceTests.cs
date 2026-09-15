using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using TVT.Core.Enums;
using Xunit;

namespace ExamHub.Tests;

// ── Hand-rolled fakes (no mocking library in this repo — xunit only) ──────

/// <summary>Fake IExamSessionRepository backed by in-memory lists. Shared by Task 1 (management
/// methods) and Task 2 (StartAsync) tests in this file.</summary>
file sealed class FakeExamSessionRepository : IExamSessionRepository
{
    public List<ExamSession> Sessions { get; } = [];
    public List<ExamSessionExam> PoolExams { get; } = [];
    public List<ExamSessionAssignment> Assignments { get; } = [];
    public List<ExamSubmission> Submissions { get; } = [];
    public HashSet<Guid> AssignedStudentIds { get; } = [];

    public Task<ExamSession?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var s = Sessions.FirstOrDefault(x => x.Id == id);
        if (s is not null)
        {
            s.Exams = PoolExams.Where(e => e.SessionId == id).ToList();
            s.Assignments = Assignments.Where(a => a.SessionId == id).ToList();
        }
        return Task.FromResult(s);
    }

    public Task<(IReadOnlyList<ExamSession> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? subjectId, int? gradeLevelId,
        ExamSessionStatusEnum? status, string? keyword, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<ExamSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Sessions.FirstOrDefault(x => x.Id == id));

    public Task AddAsync(ExamSession s, CancellationToken ct = default)
    {
        Sessions.Add(s);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ExamSession s, CancellationToken ct = default) => Task.CompletedTask;

    public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();

    public Task SetStatusAsync(Guid id, ExamSessionStatusEnum status, CancellationToken ct = default)
    {
        Sessions.First(x => x.Id == id).Status = status;
        return Task.CompletedTask;
    }

    public Task AddExamsAsync(Guid sessionId, IEnumerable<Guid> examIds, CancellationToken ct = default)
    {
        foreach (var examId in examIds)
            PoolExams.Add(new ExamSessionExam { SessionId = sessionId, ExamId = examId });
        return Task.CompletedTask;
    }

    public Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<IReadOnlyList<Exam>> GetPoolExamsAsync(Guid sessionId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Exam>>(
            PoolExams.Where(e => e.SessionId == sessionId && e.Exam is not null).Select(e => e.Exam!).ToList());

    public Task<bool> PoolContainsAsync(Guid sessionId, Guid examId, CancellationToken ct = default) => throw new NotSupportedException();

    public Task AddAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default)
    {
        Assignments.Add(a);
        return Task.CompletedTask;
    }

    public Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<ExamSessionAssignment?> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<int> CountStudentsForAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<IReadOnlyList<ExamSession>> GetAssignedToStudentAsync(Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<bool> IsStudentAssignedAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => Task.FromResult(AssignedStudentIds.Contains(studentId));

    public Task<int> CountSubmittedAttemptsAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => Task.FromResult(Submissions.Count(x =>
            x.SessionId == sessionId && x.StudentId == studentId && x.Status != SubmissionStatusEnum.InProgress));

    public Task<ExamSubmission?> GetInProgressAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => Task.FromResult(Submissions.FirstOrDefault(x =>
            x.SessionId == sessionId && x.StudentId == studentId && x.Status == SubmissionStatusEnum.InProgress));

    public Task<IReadOnlyList<ExamSubmission>> GetStudentSubmissionsAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ExamSubmission>>(
            Submissions.Where(x => x.SessionId == sessionId && x.StudentId == studentId).ToList());

    public Task CreateSubmissionAsync(ExamSubmission submission, CancellationToken ct = default)
    {
        Submissions.Add(submission);
        return Task.CompletedTask;
    }
}

/// <summary>Fake IExamRepository — only GetByIdAsync is exercised (SetExamsAsync pool validation).</summary>
file sealed class FakeExamRepository : IExamRepository
{
    public List<Exam> Items { get; } = [];

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

    public Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Exam>> GetBySubjectAsync(int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Exam>> GetByStatusAsync(ExamStatusEnum status, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? gradeLevelId = null, int? subjectId = null,
        ExamStatusEnum? status = null, string? keyword = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> UpdateStatusAsync(Guid id, ExamStatusEnum status, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Exam>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Exam>> GetAsync(System.Linq.Expressions.Expression<Func<Exam, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Exam?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Exam, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Exam, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Exam, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Exam> AddAsync(Exam entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<Exam> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(Exam entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(Exam entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

public class ExamSessionServiceManagementTests
{
    private static CreateExamSessionRequest ValidCreateRequest() => new()
    {
        Title = "Kiểm tra giữa kỳ", SubjectId = 1, GradeLevelId = 1,
        OpenAt = DateTime.UtcNow.AddDays(1), CloseAt = DateTime.UtcNow.AddDays(2),
        MaxAttempts = 1, PickMode = "Random",
    };

    [Fact]
    public async Task CreateAsync_CloseAtBeforeOpenAt_ReturnsError()
    {
        var service = new ExamSessionService(new FakeExamSessionRepository(), new FakeExamRepository());
        var req = ValidCreateRequest() with { OpenAt = DateTime.UtcNow.AddDays(2), CloseAt = DateTime.UtcNow.AddDays(1) };

        var result = await service.CreateAsync(req, "teacher1");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Thời điểm đóng phải sau thời điểm mở.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessWithNewId()
    {
        var repo = new FakeExamSessionRepository();
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.CreateAsync(ValidCreateRequest(), "teacher1");

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.Single(repo.Sessions);
    }

    [Fact]
    public async Task UpdateAsync_SessionNotFound_ReturnsError()
    {
        var service = new ExamSessionService(new FakeExamSessionRepository(), new FakeExamRepository());
        var req = new UpdateExamSessionRequest
        {
            Title = "x", SubjectId = 1, GradeLevelId = 1,
            OpenAt = DateTime.UtcNow.AddDays(1), CloseAt = DateTime.UtcNow.AddDays(2), MaxAttempts = 1, PickMode = "Random",
        };

        var result = await service.UpdateAsync(Guid.NewGuid(), req, "teacher1");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Không tìm thấy kỳ thi.", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_SessionClosed_ReturnsError()
    {
        var repo = new FakeExamSessionRepository();
        var id = Guid.NewGuid();
        repo.Sessions.Add(new ExamSession
        {
            Id = id, Title = "old", SubjectId = 1, GradeLevelId = 1,
            OpenAt = DateTime.UtcNow.AddDays(-2), CloseAt = DateTime.UtcNow.AddDays(-1),
            Status = ExamSessionStatusEnum.Closed,
        });
        var service = new ExamSessionService(repo, new FakeExamRepository());
        var req = new UpdateExamSessionRequest
        {
            Title = "new", SubjectId = 1, GradeLevelId = 1,
            OpenAt = DateTime.UtcNow.AddDays(1), CloseAt = DateTime.UtcNow.AddDays(2), MaxAttempts = 1, PickMode = "Random",
        };

        var result = await service.UpdateAsync(id, req, "teacher1");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Kỳ thi đã đóng, không thể sửa.", result.Message);
    }

    [Fact]
    public async Task PublishAsync_EmptyPool_ReturnsError()
    {
        var repo = new FakeExamSessionRepository();
        var id = Guid.NewGuid();
        repo.Sessions.Add(new ExamSession
        {
            Id = id, Title = "s", SubjectId = 1, GradeLevelId = 1,
            OpenAt = DateTime.UtcNow, CloseAt = DateTime.UtcNow.AddDays(1),
        });
        repo.Assignments.Add(new ExamSessionAssignment { SessionId = id, CohortId = 1 });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.PublishAsync(id);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Kỳ thi chưa có đề trong pool.", result.Message);
    }

    [Fact]
    public async Task PublishAsync_ValidSession_ReturnsSuccessAndSetsPublished()
    {
        var repo = new FakeExamSessionRepository();
        var id = Guid.NewGuid();
        repo.Sessions.Add(new ExamSession
        {
            Id = id, Title = "s", SubjectId = 1, GradeLevelId = 1,
            OpenAt = DateTime.UtcNow, CloseAt = DateTime.UtcNow.AddDays(1),
        });
        repo.PoolExams.Add(new ExamSessionExam { SessionId = id, ExamId = Guid.NewGuid() });
        repo.Assignments.Add(new ExamSessionAssignment { SessionId = id, CohortId = 1 });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.PublishAsync(id);

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal(ExamSessionStatusEnum.Published, repo.Sessions.Single().Status);
    }

    [Fact]
    public async Task SetExamsAsync_ExamNotPublished_ReturnsError()
    {
        var repo = new FakeExamSessionRepository();
        var examRepo = new FakeExamRepository();
        var sessionId = Guid.NewGuid();
        repo.Sessions.Add(new ExamSession { Id = sessionId, Title = "s", SubjectId = 1, GradeLevelId = 1, OpenAt = DateTime.UtcNow, CloseAt = DateTime.UtcNow.AddDays(1) });
        var examId = Guid.NewGuid();
        examRepo.Items.Add(new Exam { Id = examId, Title = "de", SubjectId = 1, GradeLevelId = 1, Status = ExamStatusEnum.Draft });
        var service = new ExamSessionService(repo, examRepo);

        var result = await service.SetExamsAsync(sessionId, [examId], "teacher1");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Đề 'de' chưa phát hành.", result.Message);
    }

    [Fact]
    public async Task SetExamsAsync_ValidExam_ReturnsSuccessAndAddsToPool()
    {
        var repo = new FakeExamSessionRepository();
        var examRepo = new FakeExamRepository();
        var sessionId = Guid.NewGuid();
        repo.Sessions.Add(new ExamSession { Id = sessionId, Title = "s", SubjectId = 1, GradeLevelId = 1, OpenAt = DateTime.UtcNow, CloseAt = DateTime.UtcNow.AddDays(1) });
        var examId = Guid.NewGuid();
        examRepo.Items.Add(new Exam { Id = examId, Title = "de", SubjectId = 1, GradeLevelId = 1, Status = ExamStatusEnum.Published });
        var service = new ExamSessionService(repo, examRepo);

        var result = await service.SetExamsAsync(sessionId, [examId], "teacher1");

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Single(repo.PoolExams);
    }

    [Fact]
    public async Task AddAssignmentAsync_BothCohortAndClassGiven_ReturnsError()
    {
        var repo = new FakeExamSessionRepository();
        var sessionId = Guid.NewGuid();
        repo.Sessions.Add(new ExamSession { Id = sessionId, Title = "s", SubjectId = 1, GradeLevelId = 1, OpenAt = DateTime.UtcNow, CloseAt = DateTime.UtcNow.AddDays(1) });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.AddAssignmentAsync(sessionId, new CreateAssignmentRequest(1, 1));

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Chọn đúng một trong hai: khoá hoặc lớp.", result.Message);
    }

    [Fact]
    public async Task AddAssignmentAsync_ValidCohort_ReturnsSuccessWithNewId()
    {
        var repo = new FakeExamSessionRepository();
        var sessionId = Guid.NewGuid();
        repo.Sessions.Add(new ExamSession { Id = sessionId, Title = "s", SubjectId = 1, GradeLevelId = 1, OpenAt = DateTime.UtcNow, CloseAt = DateTime.UtcNow.AddDays(1) });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.AddAssignmentAsync(sessionId, new CreateAssignmentRequest(1, null));

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.Single(repo.Assignments);
    }
}
