# Business-Rule Error Response Normalization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make every business-rule validation failure in `ExamSessionService`, `CohortMemberService`, `CohortClassTeacherService`, and `ExportService` return `RequestResponse<T>.Error(...)` instead of throwing `InvalidOperationException`, so the frontend actually receives the shape it already knows how to read (matching the existing `AuthService` pattern) instead of the global exception middleware's incompatible JSON shape.

**Architecture:** Each throwing method's return type changes from `Task<X>` (or `Task`) to `Task<RequestResponse<X>>` (or `Task<RequestResponse<bool>>`). Every `throw new InvalidOperationException(msg)` becomes `return RequestResponse<X>.Error(msg);`; every `x ?? throw new InvalidOperationException(msg)` splits into an explicit null check with an early `return`. Controllers collapse to a straight `return Ok(await service.X(...));` pass-through (like `AuthController`), since the response already carries success/error internally.

**Tech Stack:** ASP.NET Core (C#), xunit (hand-rolled fakes, no mocking library), `TVT.Core.RequestResponse<T>` / `TVT.Core.Enums.RequestResponseStatus`.

**Spec:** `docs/superpowers/specs/2026-09-15-error-response-normalization-design.md`

## Global Constraints

- Convert `throw new InvalidOperationException(msg)` → `return RequestResponse<X>.Error(msg);`. Convert `x ?? throw new InvalidOperationException(msg)` → explicit null check + early `return RequestResponse<X>.Error(msg);`.
- A currently-`void` `Task` method becomes `Task<RequestResponse<bool>>`, returning `RequestResponse<bool>.Success(msg, true, 1)` on the happy path.
- HTTP status for these business-rule failures stays **200** (`Ok(...)`) — matches `AuthController`'s convention; the frontend reads `status` from the JSON body, not the HTTP status code, for this class of error.
- The two actions that returned HTTP 201 on success today (`ExamSessionController.Create`, `ExamSessionController.AddAssignment`) keep doing so — branch on `result.Status == RequestResponseStatus.Success` to pick `StatusCode(201, result)` vs `Ok(result)`.
- Drop any existing controller-level `try/catch (InvalidOperationException)` around calls into converted methods (only `CohortMemberController.AddStudent` has one today) — it becomes dead code once the service stops throwing.
- Out of scope, do not touch: `ExamSubmissionService.SaveProgressAsync` (already correctly handled via controller try/catch → 403/409), `DependencyContainer.cs:59` (startup-time config validation, not a request), `ExamHub.Tests/SubmissionFlowTests.cs:85` (unrelated test-fake code simulating a DB unique-constraint violation).
- No frontend changes required anywhere in this plan.
- Tests: this repo has no mocking library — hand-rolled fakes implement the full repository interface; members the test doesn't exercise throw `NotSupportedException()` (see `FakeSubmissionRepository` in `ExamHub.Tests/SubmissionFlowTests.cs` for the established pattern).

---

### Task 1: `ExamSessionService` — management methods (Create/Update/Publish/SetExams/AddAssignment)

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Application/Services/IExamSessionService.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs:14-48,92-103,111-125,133-149`
- Modify: `exam_hub_api/ExamHub.API/Controllers/Exam/ExamSessionController.cs:59-64,71-76,94-99,118-134`
- Test: `exam_hub_api/ExamHub.Tests/ExamSessionServiceTests.cs` (new file)

**Interfaces:**
- Consumes: `RequestResponse<T>` / `RequestResponseStatus` (`TVT.Core`, `TVT.Core.Enums`) — already used throughout the codebase, no new dependency.
- Produces (for Task 2, which lives in the same files): the same `FakeExamSessionRepository` / `FakeExamRepository` test fakes defined here, extended in Task 2 rather than redefined.

- [ ] **Step 1: Write the failing tests**

Create `exam_hub_api/ExamHub.Tests/ExamSessionServiceTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run the tests to verify they fail to compile (return types don't match yet)**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter ExamSessionServiceManagementTests -v q` (from `exam_hub_api/`)
Expected: build error — `ExamSessionService.CreateAsync` etc. still return `Task<Guid>`/`Task`, not `Task<RequestResponse<...>>`.

- [ ] **Step 3: Update `IExamSessionService`**

In `exam_hub_api/ExamHub.Core/Application/Services/IExamSessionService.cs`, add `using TVT.Core;` and change these 5 signatures:

```csharp
Task<RequestResponse<Guid>> CreateAsync(CreateExamSessionRequest req, string by, CancellationToken ct = default);

Task<RequestResponse<bool>> UpdateAsync(Guid id, UpdateExamSessionRequest req, string by, CancellationToken ct = default);

Task<RequestResponse<bool>> PublishAsync(Guid id, CancellationToken ct = default);

Task<RequestResponse<bool>> SetExamsAsync(Guid sessionId, IReadOnlyList<Guid> examIds, string by, CancellationToken ct = default);

Task<RequestResponse<Guid>> AddAssignmentAsync(Guid sessionId, CreateAssignmentRequest req, CancellationToken ct = default);
```

Leave `DeleteAsync`, `CloseAsync`, `RemoveExamAsync`, `RemoveAssignmentAsync`, `GetDetailAsync`, `GetPagedAsync`, `GetAssignmentByIdAsync`, `GetMySessionsAsync`, `GetPoolForStudentAsync`, `StartAsync` untouched (`StartAsync` is Task 2).

- [ ] **Step 4: Update `ExamSessionService`**

In `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs`, add `using TVT.Core;` and replace these 5 methods:

```csharp
/// <inheritdoc/>
public async Task<RequestResponse<Guid>> CreateAsync(CreateExamSessionRequest req, string by, CancellationToken ct = default)
{
    if (req.CloseAt.ToUniversalTime() <= req.OpenAt.ToUniversalTime())
        return RequestResponse<Guid>.Error("Thời điểm đóng phải sau thời điểm mở.");
    var entity = req.ToEntity();
    entity.CreatedBy = by;
    entity.ModifiedBy = by;
    entity.Created = DateTime.UtcNow;
    entity.Modified = DateTime.UtcNow;
    await _repo.AddAsync(entity, ct);
    return RequestResponse<Guid>.Success("Tạo kỳ thi thành công!", entity.Id, 1);
}

/// <inheritdoc/>
public async Task<RequestResponse<bool>> UpdateAsync(Guid id, UpdateExamSessionRequest req, string by, CancellationToken ct = default)
{
    var entity = await _repo.GetByIdAsync(id, ct);
    if (entity is null) return RequestResponse<bool>.Error("Không tìm thấy kỳ thi.");
    if (entity.Status == ExamSessionStatusEnum.Closed)
        return RequestResponse<bool>.Error("Kỳ thi đã đóng, không thể sửa.");
    if (req.CloseAt.ToUniversalTime() <= req.OpenAt.ToUniversalTime())
        return RequestResponse<bool>.Error("Thời điểm đóng phải sau thời điểm mở.");

    entity.Title = req.Title;
    entity.Description = req.Description;
    entity.SubjectId = req.SubjectId;
    entity.GradeLevelId = req.GradeLevelId;
    entity.OpenAt = req.OpenAt.ToUniversalTime();
    entity.CloseAt = req.CloseAt.ToUniversalTime();
    entity.MaxAttempts = req.MaxAttempts;
    entity.PickMode = Enum.Parse<ExamSessionPickModeEnum>(req.PickMode);
    entity.ModifiedBy = by;
    entity.Modified = DateTime.UtcNow;
    await _repo.UpdateAsync(entity, ct);
    return RequestResponse<bool>.Success("Cập nhật kỳ thi thành công!", true, 1);
}
```

```csharp
/// <inheritdoc/>
public async Task<RequestResponse<bool>> PublishAsync(Guid id, CancellationToken ct = default)
{
    var s = await _repo.GetDetailAsync(id, ct);
    if (s is null) return RequestResponse<bool>.Error("Không tìm thấy kỳ thi.");
    if (s.Exams.Count == 0)
        return RequestResponse<bool>.Error("Kỳ thi chưa có đề trong pool.");
    if (s.Assignments.Count == 0)
        return RequestResponse<bool>.Error("Kỳ thi chưa được giao cho lớp/khoá nào.");
    if (s.CloseAt <= DateTime.UtcNow)
        return RequestResponse<bool>.Error("Thời điểm đóng phải ở tương lai.");
    await _repo.SetStatusAsync(id, ExamSessionStatusEnum.Published, ct);
    return RequestResponse<bool>.Success("Phát hành kỳ thi thành công!", true, 1);
}
```

```csharp
/// <inheritdoc/>
public async Task<RequestResponse<bool>> SetExamsAsync(Guid sessionId, IReadOnlyList<Guid> examIds, string by, CancellationToken ct = default)
{
    var session = await _repo.GetByIdAsync(sessionId, ct);
    if (session is null) return RequestResponse<bool>.Error("Không tìm thấy kỳ thi.");
    foreach (var examId in examIds.Distinct())
    {
        var exam = await _examRepo.GetByIdAsync(examId, ct);
        if (exam is null) return RequestResponse<bool>.Error($"Không tìm thấy đề {examId}.");
        if (exam.Status != ExamStatusEnum.Published)
            return RequestResponse<bool>.Error($"Đề '{exam.Title}' chưa phát hành.");
        if (exam.SubjectId != session.SubjectId || exam.GradeLevelId != session.GradeLevelId)
            return RequestResponse<bool>.Error($"Đề '{exam.Title}' không cùng môn/cấp lớp với kỳ thi.");
    }
    await _repo.AddExamsAsync(sessionId, examIds, ct);
    return RequestResponse<bool>.Success("Cập nhật đề thi thành công!", true, 1);
}
```

```csharp
/// <inheritdoc/>
public async Task<RequestResponse<Guid>> AddAssignmentAsync(Guid sessionId, CreateAssignmentRequest req, CancellationToken ct = default)
{
    var session = await _repo.GetByIdAsync(sessionId, ct);
    if (session is null) return RequestResponse<Guid>.Error("Không tìm thấy kỳ thi.");
    var hasCohort = req.CohortId is not null;
    var hasClass = req.CohortClassId is not null;
    if (hasCohort == hasClass)
        return RequestResponse<Guid>.Error("Chọn đúng một trong hai: khoá hoặc lớp.");
    var assignment = new ExamSessionAssignment
    {
        SessionId = sessionId,
        CohortId = req.CohortId,
        CohortClassId = req.CohortClassId
    };
    await _repo.AddAssignmentAsync(assignment, ct);
    return RequestResponse<Guid>.Success("Giao kỳ thi thành công!", assignment.Id, 1);
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter ExamSessionServiceManagementTests -v q` (from `exam_hub_api/`)
Expected: PASS, 10/10.

- [ ] **Step 6: Update `ExamSessionController`**

In `exam_hub_api/ExamHub.API/Controllers/Exam/ExamSessionController.cs`, add `using TVT.Core.Enums;` and replace:

```csharp
[HttpPost, Authorize(Roles = "Admin,Teacher")]
public async Task<ActionResult<RequestResponse<Guid>>> Create([FromBody] CreateExamSessionRequest request, CancellationToken ct)
{
    var result = await service.CreateAsync(request, User.GetTag(), ct);
    return result.Status == RequestResponseStatus.Success ? StatusCode(201, result) : Ok(result);
}
```

```csharp
[HttpPut("{id:guid}"), Authorize(Roles = "Admin,Teacher")]
public async Task<ActionResult<RequestResponse<bool>>> Update(Guid id, [FromBody] UpdateExamSessionRequest request, CancellationToken ct)
{
    return Ok(await service.UpdateAsync(id, request, User.GetTag(), ct));
}
```

```csharp
[HttpPost("{id:guid}/exams"), Authorize(Roles = "Admin,Teacher")]
public async Task<ActionResult<RequestResponse<bool>>> SetExams(Guid id, [FromBody] SetSessionExamsRequest request, CancellationToken ct)
{
    return Ok(await service.SetExamsAsync(id, request.ExamIds, User.GetTag(), ct));
}
```

```csharp
[HttpPost("{id:guid}/assignments"), Authorize(Roles = "Admin,Teacher")]
public async Task<ActionResult<RequestResponse<Guid>>> AddAssignment(Guid id, [FromBody] CreateAssignmentRequest request, CancellationToken ct)
{
    if (request.CohortClassId is { } cohortClassId)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, cohortClassId, "TeacherOwnsCohortClass");
        if (!authResult.Succeeded)
            return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách lớp học này."));
    }
    else if (!User.IsInRole("Admin"))
    {
        return StatusCode(403, RequestResponse<object>.Error("Chỉ Quản trị viên được giao kỳ thi cho cả khoá."));
    }

    var result = await service.AddAssignmentAsync(id, request, ct);
    return result.Status == RequestResponseStatus.Success ? StatusCode(201, result) : Ok(result);
}
```

Also update `PublishAsync`'s caller (`Publish` action, unchanged signature-wise but body simplifies):

```csharp
[HttpPost("{id:guid}/publish"), Authorize(Roles = "Admin,Teacher")]
public async Task<ActionResult<RequestResponse<bool>>> Publish(Guid id, CancellationToken ct)
{
    return Ok(await service.PublishAsync(id, ct));
}
```

- [ ] **Step 7: Build and run the full backend test suite**

Run (from `exam_hub_api/`): `dotnet build ExamHub.API/ExamHub.API.csproj -v q` then `dotnet test ExamHub.Tests/ExamHub.Tests.csproj -v q`
Expected: build succeeds (0 errors); all tests pass (existing 55 + the 10 new ones = 65).

- [ ] **Step 8: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Application/Services/IExamSessionService.cs \
        exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs \
        exam_hub_api/ExamHub.API/Controllers/Exam/ExamSessionController.cs \
        exam_hub_api/ExamHub.Tests/ExamSessionServiceTests.cs
git commit -m "refactor(api): return RequestResponse.Error instead of throwing in ExamSessionService management methods"
```

---

### Task 2: `ExamSessionService.StartAsync` — the exam attempt / retake flow

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Application/Services/IExamSessionService.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs:197-253`
- Modify: `exam_hub_api/ExamHub.API/Controllers/Exam/ExamSessionController.cs:215-222`
- Test: `exam_hub_api/ExamHub.Tests/ExamSessionServiceTests.cs` (extend — reuse the fakes from Task 1)

**Interfaces:**
- Consumes: `FakeExamSessionRepository` from Task 1 (already supports `IsStudentAssignedAsync`, `CountSubmittedAttemptsAsync`, `GetInProgressAsync`, `GetStudentSubmissionsAsync`, `GetPoolExamsAsync`, `CreateSubmissionAsync`, `AssignedStudentIds`).
- Produces: `Task<RequestResponse<StartSessionResponse>> StartAsync(...)` — used by Task 6 verification and by `ExamResultPage.tsx`'s "Làm lại" flow (already shipped, unaffected — it reads `.data`/`.status` the same way regardless of whether the backend throws or returns Error, but today it silently mis-reports "Không thể vào thi" instead of the real reason; after this task it shows the real reason).

- [ ] **Step 1: Write the failing tests**

Append to `exam_hub_api/ExamHub.Tests/ExamSessionServiceTests.cs`:

```csharp
public class ExamSessionServiceStartTests
{
    private static ExamSession OpenSession(Guid id, short maxAttempts = 1, ExamSessionPickModeEnum pickMode = ExamSessionPickModeEnum.Random) => new()
    {
        Id = id, Title = "s", SubjectId = 1, GradeLevelId = 1,
        OpenAt = DateTime.UtcNow.AddHours(-1), CloseAt = DateTime.UtcNow.AddHours(1),
        MaxAttempts = maxAttempts, PickMode = pickMode, Status = ExamSessionStatusEnum.Published,
    };

    [Fact]
    public async Task StartAsync_StudentNotAssigned_ReturnsError()
    {
        var repo = new FakeExamSessionRepository();
        var sessionId = Guid.NewGuid();
        repo.Sessions.Add(OpenSession(sessionId));
        repo.PoolExams.Add(new ExamSessionExam { SessionId = sessionId, ExamId = Guid.NewGuid(), Exam = new Exam { Id = Guid.NewGuid(), Title = "de", SubjectId = 1, GradeLevelId = 1 } });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.StartAsync(sessionId, Guid.NewGuid(), null, "student1");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Bạn không được giao kỳ thi này.", result.Message);
    }

    [Fact]
    public async Task StartAsync_NoAttemptsLeft_ReturnsError()
    {
        var repo = new FakeExamSessionRepository();
        var sessionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        repo.Sessions.Add(OpenSession(sessionId, maxAttempts: 1));
        repo.AssignedStudentIds.Add(studentId);
        repo.Submissions.Add(new ExamSubmission
        {
            Id = Guid.NewGuid(), SessionId = sessionId, ExamId = Guid.NewGuid(), StudentId = studentId,
            Status = SubmissionStatusEnum.Submitted, AttemptNo = 1, StartedAt = DateTime.UtcNow.AddMinutes(-30),
        });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.StartAsync(sessionId, studentId, null, "student1");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Bạn đã hết lượt làm bài.", result.Message);
    }

    [Fact]
    public async Task StartAsync_InProgressExists_ResumesWithoutCreatingNewSubmission()
    {
        var repo = new FakeExamSessionRepository();
        var sessionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        repo.Sessions.Add(OpenSession(sessionId));
        repo.AssignedStudentIds.Add(studentId);
        repo.Submissions.Add(new ExamSubmission
        {
            Id = submissionId, SessionId = sessionId, ExamId = examId, StudentId = studentId,
            Status = SubmissionStatusEnum.InProgress, AttemptNo = 1, StartedAt = DateTime.UtcNow.AddMinutes(-5),
        });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.StartAsync(sessionId, studentId, null, "student1");

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal(submissionId, result.Data!.SubmissionId);
        Assert.Equal(examId, result.Data!.ExamId);
        Assert.Single(repo.Submissions);
    }

    [Fact]
    public async Task StartAsync_StudentChoiceWithoutChosenExam_ReturnsError()
    {
        var repo = new FakeExamSessionRepository();
        var sessionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        repo.Sessions.Add(OpenSession(sessionId, pickMode: ExamSessionPickModeEnum.StudentChoice));
        repo.AssignedStudentIds.Add(studentId);
        var examId = Guid.NewGuid();
        repo.PoolExams.Add(new ExamSessionExam { SessionId = sessionId, ExamId = examId, Exam = new Exam { Id = examId, Title = "de", SubjectId = 1, GradeLevelId = 1 } });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.StartAsync(sessionId, studentId, null, "student1");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Vui lòng chọn đề.", result.Message);
    }

    [Fact]
    public async Task StartAsync_RandomPickValidRequest_CreatesSubmissionAndReturnsSuccess()
    {
        var repo = new FakeExamSessionRepository();
        var sessionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        repo.Sessions.Add(OpenSession(sessionId));
        repo.AssignedStudentIds.Add(studentId);
        var examId = Guid.NewGuid();
        repo.PoolExams.Add(new ExamSessionExam { SessionId = sessionId, ExamId = examId, Exam = new Exam { Id = examId, Title = "de", SubjectId = 1, GradeLevelId = 1 } });
        var service = new ExamSessionService(repo, new FakeExamRepository());

        var result = await service.StartAsync(sessionId, studentId, null, "student1");

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal(examId, result.Data!.ExamId);
        Assert.Single(repo.Submissions);
        Assert.Equal(SubmissionStatusEnum.InProgress, repo.Submissions.Single().Status);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail to compile**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter ExamSessionServiceStartTests -v q` (from `exam_hub_api/`)
Expected: build error — `StartAsync` still returns `Task<StartSessionResponse>`.

- [ ] **Step 3: Update `IExamSessionService.StartAsync` signature**

In `exam_hub_api/ExamHub.Core/Application/Services/IExamSessionService.cs`:

```csharp
Task<RequestResponse<StartSessionResponse>> StartAsync(Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken ct = default);
```

- [ ] **Step 4: Update `ExamSessionService.StartAsync`**

Replace in `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs`:

```csharp
/// <inheritdoc/>
public async Task<RequestResponse<StartSessionResponse>> StartAsync(
    Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken ct = default)
{
    var session = await _repo.GetByIdAsync(sessionId, ct);
    if (session is null) return RequestResponse<StartSessionResponse>.Error("Không tìm thấy kỳ thi.");
    if (session.Status != ExamSessionStatusEnum.Published)
        return RequestResponse<StartSessionResponse>.Error("Kỳ thi chưa mở.");
    var now = DateTime.UtcNow;
    if (now < session.OpenAt) return RequestResponse<StartSessionResponse>.Error("Kỳ thi chưa đến giờ mở.");
    if (now > session.CloseAt) return RequestResponse<StartSessionResponse>.Error("Kỳ thi đã đóng.");
    if (!await _repo.IsStudentAssignedAsync(sessionId, studentId, ct))
        return RequestResponse<StartSessionResponse>.Error("Bạn không được giao kỳ thi này.");

    // Đang có lượt dở → trả lại đúng đề đó (Tiếp tục)
    var inProgress = await _repo.GetInProgressAsync(sessionId, studentId, ct);
    if (inProgress is not null)
        return RequestResponse<StartSessionResponse>.Success(
            "Vào thi thành công!", new StartSessionResponse(inProgress.Id, inProgress.ExamId), 1);

    var used = await _repo.CountSubmittedAttemptsAsync(sessionId, studentId, ct);
    if (used >= session.MaxAttempts)
        return RequestResponse<StartSessionResponse>.Error("Bạn đã hết lượt làm bài.");

    var pool = await _repo.GetPoolExamsAsync(sessionId, ct);
    if (pool.Count == 0) return RequestResponse<StartSessionResponse>.Error("Kỳ thi chưa có đề.");

    Guid examId;
    if (session.PickMode == ExamSessionPickModeEnum.StudentChoice)
    {
        if (chosenExamId is null) return RequestResponse<StartSessionResponse>.Error("Vui lòng chọn đề.");
        if (pool.All(e => e.Id != chosenExamId.Value))
            return RequestResponse<StartSessionResponse>.Error("Đề không thuộc kỳ thi.");
        var done = await _repo.GetStudentSubmissionsAsync(sessionId, studentId, ct);
        if (done.Any(s => s.ExamId == chosenExamId.Value && s.Status != SubmissionStatusEnum.InProgress))
            return RequestResponse<StartSessionResponse>.Error("Bạn đã làm đề này rồi.");
        examId = chosenExamId.Value;
    }
    else
    {
        examId = pool[Random.Shared.Next(pool.Count)].Id;
    }

    var submission = new ExamSubmission
    {
        Id = Guid.NewGuid(),
        SessionId = sessionId,
        ExamId = examId,
        StudentId = studentId,
        Status = SubmissionStatusEnum.InProgress,
        AttemptNo = (short)(used + 1),
        StartedAt = now,
        CreatedBy = by,
        ModifiedBy = by,
        Modified = DateTime.UtcNow
    };
    await _repo.CreateSubmissionAsync(submission, ct);
    return RequestResponse<StartSessionResponse>.Success(
        "Vào thi thành công!", new StartSessionResponse(submission.Id, examId), 1);
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter ExamSessionServiceStartTests -v q` (from `exam_hub_api/`)
Expected: PASS, 5/5.

- [ ] **Step 6: Update `ExamSessionController.Start`**

In `exam_hub_api/ExamHub.API/Controllers/Exam/ExamSessionController.cs`:

```csharp
[HttpPost("{id:guid}/start")]
public async Task<ActionResult<RequestResponse<StartSessionResponse>>> Start(Guid id, [FromBody] StartSessionRequest request, CancellationToken ct)
{
    if (CurrentUser.UserId.IsNullOrEmpty())
        return StatusCode(401, RequestResponse<StartSessionResponse>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
    return Ok(await service.StartAsync(id, CurrentUser.UserId!.Value, request.ExamId, User.GetTag(), ct));
}
```

(The `.Success("Vào thi thành công!", ...)` message baked into the service replaces the one previously hard-coded in the controller — the controller no longer wraps the result in its own `RequestResponse<...>.Success(...)`.)

- [ ] **Step 7: Build and run the full backend test suite**

Run (from `exam_hub_api/`): `dotnet build ExamHub.API/ExamHub.API.csproj -v q` then `dotnet test ExamHub.Tests/ExamHub.Tests.csproj -v q`
Expected: build succeeds; all tests pass (65 + 5 = 70).

- [ ] **Step 8: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Application/Services/IExamSessionService.cs \
        exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs \
        exam_hub_api/ExamHub.API/Controllers/Exam/ExamSessionController.cs \
        exam_hub_api/ExamHub.Tests/ExamSessionServiceTests.cs
git commit -m "refactor(api): return RequestResponse.Error instead of throwing in ExamSessionService.StartAsync"
```

---

### Task 3: `CohortMemberService` — `AddStudentAsync` / `SetSectionAsync` / `ValidateSectionAsync`

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberService.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortMemberService.cs`
- Modify: `exam_hub_api/ExamHub.API/Controllers/School/CohortMemberController.cs:63-80,110-115`
- Test: `exam_hub_api/ExamHub.Tests/CohortMemberServiceTests.cs` (new file)

**Interfaces:**
- Consumes: `RequestResponse<T>` / `RequestResponseStatus`.
- Produces: `Task<RequestResponse<CohortMember>> AddStudentAsync(...)` (still returns the entity, wrapped — the controller maps entity → `CohortMemberResponse` DTO after unwrapping, same as it does today) and `Task<RequestResponse<bool>> SetSectionAsync(...)`.

- [ ] **Step 1: Write the failing tests**

Create `exam_hub_api/ExamHub.Tests/CohortMemberServiceTests.cs`:

```csharp
using System.Linq.Expressions;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using TVT.Core.Enums;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeCohortMemberRepository : ICohortMemberRepository
{
    public List<CohortMember> Items { get; } = [];

    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember?> GetByCohortAndStudentAsync(int cohortId, Guid studentId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
    {
        Items.First(x => x.Id == id).Section = section;
        return Task.FromResult(true);
    }

    public Task<bool> ExistsActiveMembershipAsync(int cohortId, Guid studentId, CancellationToken ct = default)
        => Task.FromResult(Items.Any(x => x.CohortId == cohortId && x.StudentId == studentId && x.IsActive));

    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<CohortMember>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<CohortMember>> GetAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<CohortMember?> FirstOrDefaultAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<CohortMember, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<CohortMember, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();

    public Task<CohortMember> AddAsync(CohortMember entity, CancellationToken ct = default)
    {
        Items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task AddRangeAsync(IEnumerable<CohortMember> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(CohortMember entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

file sealed class FakeCohortRepository : ICohortRepository
{
    public List<Cohort> Items { get; } = [];

    public Task<Cohort?> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Cohort>> GetAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort?> FirstOrDefaultAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<Cohort, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<Cohort, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Cohort> AddAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<Cohort> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(Cohort entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

public class CohortMemberServiceTests
{
    [Fact]
    public async Task AddStudentAsync_AlreadyActiveMember_ReturnsError()
    {
        var repo = new FakeCohortMemberRepository();
        var studentId = Guid.NewGuid();
        repo.Items.Add(new CohortMember { Id = Guid.NewGuid(), CohortId = 1, StudentId = studentId, IsActive = true });
        var service = new CohortMemberService(repo, new FakeCohortRepository());

        var result = await service.AddStudentAsync(new CohortMember { CohortId = 1, StudentId = studentId });

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Học sinh đã thuộc lớp khác trong khối này.", result.Message);
    }

    [Fact]
    public async Task AddStudentAsync_SectionOutOfRange_ReturnsError()
    {
        var repo = new FakeCohortMemberRepository();
        var cohortRepo = new FakeCohortRepository();
        cohortRepo.Items.Add(new Cohort { Id = 1, Name = "K1", SchoolId = 1, NumClasses = 2 });
        var service = new CohortMemberService(repo, cohortRepo);

        var result = await service.AddStudentAsync(new CohortMember { CohortId = 1, StudentId = Guid.NewGuid(), Section = "C" });

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Lớp 'C' không hợp lệ cho khoá này (chỉ A..B).", result.Message);
    }

    [Fact]
    public async Task AddStudentAsync_ValidStudent_ReturnsSuccessAndAdds()
    {
        var repo = new FakeCohortMemberRepository();
        var cohortRepo = new FakeCohortRepository();
        cohortRepo.Items.Add(new Cohort { Id = 1, Name = "K1", SchoolId = 1, NumClasses = 2 });
        var service = new CohortMemberService(repo, cohortRepo);

        var result = await service.AddStudentAsync(new CohortMember { CohortId = 1, StudentId = Guid.NewGuid(), Section = "A" });

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Single(repo.Items);
    }

    [Fact]
    public async Task SetSectionAsync_MemberNotFound_ReturnsError()
    {
        var service = new CohortMemberService(new FakeCohortMemberRepository(), new FakeCohortRepository());

        var result = await service.SetSectionAsync(Guid.NewGuid(), "A");

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Không tìm thấy học sinh trong khoá.", result.Message);
    }

    [Fact]
    public async Task SetSectionAsync_ValidSection_ReturnsSuccessAndUpdates()
    {
        var repo = new FakeCohortMemberRepository();
        var cohortRepo = new FakeCohortRepository();
        var memberId = Guid.NewGuid();
        repo.Items.Add(new CohortMember { Id = memberId, CohortId = 1, StudentId = Guid.NewGuid() });
        cohortRepo.Items.Add(new Cohort { Id = 1, Name = "K1", SchoolId = 1, NumClasses = 2 });
        var service = new CohortMemberService(repo, cohortRepo);

        var result = await service.SetSectionAsync(memberId, "b");

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal("B", repo.Items.Single().Section);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail to compile**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter CohortMemberServiceTests -v q` (from `exam_hub_api/`)
Expected: build error — `AddStudentAsync` returns `Task<CohortMember>`, `SetSectionAsync` returns `Task<bool>`.

- [ ] **Step 3: Update `ICohortMemberService`**

In `exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberService.cs`, add `using TVT.Core;` and change:

```csharp
Task<RequestResponse<CohortMember>> AddStudentAsync(CohortMember entity, CancellationToken ct = default);
Task<RequestResponse<bool>> SetSectionAsync(Guid id, string? section, CancellationToken ct = default);
```

- [ ] **Step 4: Update `CohortMemberService`**

Replace in `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortMemberService.cs` (add `using TVT.Core;`):

```csharp
public async Task<RequestResponse<CohortMember>> AddStudentAsync(CohortMember entity, CancellationToken ct = default)
{
    if (await _repo.ExistsActiveMembershipAsync(entity.CohortId, entity.StudentId, ct))
        return RequestResponse<CohortMember>.Error("Học sinh đã thuộc lớp khác trong khối này.");
    entity.Section = NormalizeSection(entity.Section);
    var sectionError = await ValidateSectionAsync(entity.CohortId, entity.Section, ct);
    if (sectionError is not null) return RequestResponse<CohortMember>.Error(sectionError);
    entity.Id       = Guid.NewGuid();
    entity.JoinedAt = DateOnly.FromDateTime(DateTime.UtcNow);
    entity.Created  = DateTime.UtcNow;
    entity.Modified = DateTime.UtcNow;
    var added = await _repo.AddAsync(entity, ct);
    return RequestResponse<CohortMember>.Success("Thêm học sinh thành công!", added, 1);
}

public Task RemoveStudentAsync(Guid id, CancellationToken ct = default)
    => _repo.DeleteByIdAsync(id, ct);

public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    => _repo.SetActiveAsync(id, isActive, ct);

public async Task<RequestResponse<bool>> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
{
    var member = await _repo.GetByIdAsync(id, ct);
    if (member is null) return RequestResponse<bool>.Error("Không tìm thấy học sinh trong khoá.");
    section = NormalizeSection(section);
    var sectionError = await ValidateSectionAsync(member.CohortId, section, ct);
    if (sectionError is not null) return RequestResponse<bool>.Error(sectionError);
    var updated = await _repo.SetSectionAsync(id, section, ct);
    return RequestResponse<bool>.Success("Cập nhật lớp thành công!", updated, 1);
}

// ── Helpers ─────────────────────────────────────────────────
private static string? NormalizeSection(string? section)
    => string.IsNullOrWhiteSpace(section) ? null : section.Trim().ToUpperInvariant();

/// <summary>Validates the section against the cohort's class range. Returns null when valid,
/// or the Vietnamese error message when not.</summary>
private async Task<string?> ValidateSectionAsync(int cohortId, string? section, CancellationToken ct)
{
    if (section is null) return null; // chưa xếp lớp — hợp lệ
    var cohort = await _cohortRepo.GetByIdAsync(cohortId, ct);
    if (cohort is null) return "Không tìm thấy khoá học.";
    var allowed = Enumerable.Range(0, cohort.NumClasses)
        .Select(i => ((char)('A' + i)).ToString());
    return allowed.Contains(section)
        ? null
        : $"Lớp '{section}' không hợp lệ cho khoá này (chỉ A..{(char)('A' + cohort.NumClasses - 1)}).";
}
```

(`GetByCohortAsync`, `GetBySchoolAsync`, `GetByStudentAsync`, `GetByIdAsync` at the top of the class are untouched — leave them exactly as they are today.)

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter CohortMemberServiceTests -v q` (from `exam_hub_api/`)
Expected: PASS, 5/5.

- [ ] **Step 6: Update `CohortMemberController`**

In `exam_hub_api/ExamHub.API/Controllers/School/CohortMemberController.cs`, replace:

```csharp
[HttpPost("")]
public async Task<ActionResult<RequestResponse<CohortMemberResponse>>> AddStudent([FromBody] CohortMemberRequest request, CancellationToken ct = default)
{
    var entity = request.ToEntity();
    var result = await service.AddStudentAsync(entity, ct);
    if (result.Status == TVT.Core.Enums.RequestResponseStatus.Error)
        return Ok(RequestResponse<CohortMemberResponse>.Error(result.Message!));
    return Ok(RequestResponse<CohortMemberResponse>.Success(result.Message!, CohortMemberResponse.FromEntity(result.Data!), 1));
}
```

```csharp
[HttpPatch("{id:guid}/section")]
public async Task<ActionResult<RequestResponse<bool>>> SetSection(Guid id, [FromBody] string? section, CancellationToken ct = default)
{
    return Ok(await service.SetSectionAsync(id, section, ct));
}
```

(`AddStudent` still needs to map the entity → `CohortMemberResponse` DTO on success, so it can't be a pure one-line pass-through like the others; the `try/catch` is gone, replaced by an explicit status check.)

- [ ] **Step 7: Build and run the full backend test suite**

Run (from `exam_hub_api/`): `dotnet build ExamHub.API/ExamHub.API.csproj -v q` then `dotnet test ExamHub.Tests/ExamHub.Tests.csproj -v q`
Expected: build succeeds; all tests pass (70 + 5 = 75).

- [ ] **Step 8: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberService.cs \
        exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortMemberService.cs \
        exam_hub_api/ExamHub.API/Controllers/School/CohortMemberController.cs \
        exam_hub_api/ExamHub.Tests/CohortMemberServiceTests.cs
git commit -m "refactor(api): return RequestResponse.Error instead of throwing in CohortMemberService"
```

---

### Task 4: `CohortClassTeacherService.AssignAsync`

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortClassTeacherService.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortClassTeacherService.cs`
- Modify: `exam_hub_api/ExamHub.API/Controllers/School/CohortClassTeacherController.cs:43-54`
- Test: `exam_hub_api/ExamHub.Tests/CohortClassTeacherServiceTests.cs` (extend existing file)

**Interfaces:**
- Consumes: `FakeCohortClassTeacherRepository` already defined in `CohortClassTeacherServiceTests.cs` — its `GetEligibleTeacherIdsAsync` and `ExistsAsync` stubs (currently `throw new NotSupportedException()`) need real implementations for the new tests.
- Produces: `Task<RequestResponse<CohortClassTeacher>> AssignAsync(...)`.

- [ ] **Step 1: Write the failing tests**

In `exam_hub_api/ExamHub.Tests/CohortClassTeacherServiceTests.cs`, replace the two stubbed fake methods and add a new test class. First, change:

```csharp
    public Task<bool> ExistsAsync(Expression<Func<CohortClassTeacher, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
```

to:

```csharp
    public Task<bool> ExistsAsync(Expression<Func<CohortClassTeacher, bool>> predicate, CancellationToken ct = default)
        => Task.FromResult(Items.Any(predicate.Compile()));
```

and change:

```csharp
    public Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
```

to:

```csharp
    public List<Guid> EligibleTeacherIds { get; } = [];
    public Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Guid>>(EligibleTeacherIds);
```

(the second replaces the `throw new NotSupportedException()` line and also adds the backing list right above it — also change `Task<CohortClassTeacher> AddAsync(CohortClassTeacher entity, CancellationToken ct = default) => throw new NotSupportedException();` to:

```csharp
    public Task<CohortClassTeacher> AddAsync(CohortClassTeacher entity, CancellationToken ct = default)
    {
        entity.Id = Items.Count == 0 ? 1 : Items.Max(x => x.Id) + 1;
        Items.Add(entity);
        return Task.FromResult(entity);
    }
```

Then append at the end of the file, before the final closing — actually append a new top-level class after `CohortClassTeacherServiceGetByTeacherTests`:

```csharp
public class CohortClassTeacherServiceAssignTests
{
    [Fact]
    public async Task AssignAsync_TeacherNotEligible_ReturnsError()
    {
        var repo = new FakeCohortClassTeacherRepository();
        var service = new CohortClassTeacherService(repo);

        var result = await service.AssignAsync(10, 1, Guid.NewGuid());

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Giáo viên không hợp lệ cho môn học / trường này.", result.Message);
    }

    [Fact]
    public async Task AssignAsync_AlreadyAssignedToClassAndSubject_ReturnsError()
    {
        var repo = new FakeCohortClassTeacherRepository();
        var teacherId = Guid.NewGuid();
        repo.EligibleTeacherIds.Add(teacherId);
        repo.Items.Add(new CohortClassTeacher { Id = 1, CohortClassId = 10, SubjectId = 1, TeacherId = Guid.NewGuid() });
        var service = new CohortClassTeacherService(repo);

        var result = await service.AssignAsync(10, 1, teacherId);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Môn học đã được phân công cho giáo viên khác trong lớp này.", result.Message);
    }

    [Fact]
    public async Task AssignAsync_ValidAssignment_ReturnsSuccessAndAdds()
    {
        var repo = new FakeCohortClassTeacherRepository();
        var teacherId = Guid.NewGuid();
        repo.EligibleTeacherIds.Add(teacherId);
        var service = new CohortClassTeacherService(repo);

        var result = await service.AssignAsync(10, 1, teacherId);

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal(teacherId, result.Data!.TeacherId);
        Assert.Single(repo.Items);
    }
}
```

Add `using TVT.Core.Enums;` to the top of the file's `using` block.

- [ ] **Step 2: Run the tests to verify they fail to compile**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter CohortClassTeacherServiceAssignTests -v q` (from `exam_hub_api/`)
Expected: build error — `AssignAsync` still returns `Task<CohortClassTeacher>`.

- [ ] **Step 3: Update `ICohortClassTeacherService`**

In `exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortClassTeacherService.cs`, add `using TVT.Core;` and change:

```csharp
/// <summary>
/// Phân công GV dạy môn cho lớp. Validate + kiểm ràng buộc (GV hợp lệ, không trùng môn/lớp)
/// trước khi ghi DB; trả về RequestResponse.Error nếu vi phạm.
/// </summary>
Task<RequestResponse<CohortClassTeacher>> AssignAsync(int cohortClassId, int subjectId, Guid teacherId, CancellationToken ct = default);
```

- [ ] **Step 4: Update `CohortClassTeacherService.AssignAsync`**

Replace in `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortClassTeacherService.cs` (add `using TVT.Core;`):

```csharp
/// <inheritdoc/>
public async Task<RequestResponse<CohortClassTeacher>> AssignAsync(int cohortClassId, int subjectId, Guid teacherId, CancellationToken ct = default)
{
    // 1) Validate dữ liệu đầu vào
    if (cohortClassId <= 0 || subjectId <= 0 || teacherId == Guid.Empty)
        return RequestResponse<CohortClassTeacher>.Error("Dữ liệu phân công không hợp lệ.");

    // 2) Ràng buộc: GV phải hợp lệ (thành viên trường role Teacher + dạy đúng môn)
    var eligible = await _repo.GetEligibleTeacherIdsAsync(cohortClassId, subjectId, ct);
    if (!eligible.Contains(teacherId))
        return RequestResponse<CohortClassTeacher>.Error("Giáo viên không hợp lệ cho môn học / trường này.");

    // 3) Ràng buộc: 1 môn/lớp = 1 GV
    var duplicated = await _repo.ExistsAsync(
        x => x.CohortClassId == cohortClassId && x.SubjectId == subjectId, ct);
    if (duplicated)
        return RequestResponse<CohortClassTeacher>.Error("Môn học đã được phân công cho giáo viên khác trong lớp này.");

    // 4) Hợp lệ → ghi DB
    var added = await _repo.AddAsync(
        new CohortClassTeacher { CohortClassId = cohortClassId, SubjectId = subjectId, TeacherId = teacherId }, ct);
    return RequestResponse<CohortClassTeacher>.Success("Phân công giáo viên thành công!", added, 1);
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter CohortClassTeacherServiceTests -v q` (from `exam_hub_api/`)
Expected: PASS, 5/5 (2 existing `GetByTeacherAsync` tests + 3 new `AssignAsync` tests).

- [ ] **Step 6: Update `CohortClassTeacherController.Assign`**

In `exam_hub_api/ExamHub.API/Controllers/School/CohortClassTeacherController.cs`, replace:

```csharp
[HttpPost("assign")]
[Authorize]
public async Task<ActionResult<RequestResponse<CohortClassTeacherResponse>>> Assign([FromBody] AssignTeacherRequest request, CancellationToken ct = default)
{
    var result = await service.AssignAsync(request.CohortClassId, request.SubjectId, request.TeacherId, ct);
    if (result.Status == TVT.Core.Enums.RequestResponseStatus.Error)
        return Ok(RequestResponse<CohortClassTeacherResponse>.Error(result.Message!));
    var e = result.Data!;
    var dto = new CohortClassTeacherResponse(e.Id, e.CohortClassId, e.SubjectId, e.TeacherId);
    return Ok(RequestResponse<CohortClassTeacherResponse>.Success(result.Message!, dto, 1));
}
```

- [ ] **Step 7: Build and run the full backend test suite**

Run (from `exam_hub_api/`): `dotnet build ExamHub.API/ExamHub.API.csproj -v q` then `dotnet test ExamHub.Tests/ExamHub.Tests.csproj -v q`
Expected: build succeeds; all tests pass (75 + 3 = 78).

- [ ] **Step 8: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortClassTeacherService.cs \
        exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortClassTeacherService.cs \
        exam_hub_api/ExamHub.API/Controllers/School/CohortClassTeacherController.cs \
        exam_hub_api/ExamHub.Tests/CohortClassTeacherServiceTests.cs
git commit -m "refactor(api): return RequestResponse.Error instead of throwing in CohortClassTeacherService.AssignAsync"
```

---

### Task 5: `ExportService` — `LoadExamAsync` / `UploadAsync` / `ExportPdfAsync` / `ExportDocxAsync`

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Application/Services/IExportService.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExportService.cs:35-61`
- Modify: `exam_hub_api/ExamHub.API/Controllers/Exam/ExamController.cs:130-148`
- Test: `exam_hub_api/ExamHub.Tests/ExportServiceTests.cs` (new file)

**Interfaces:**
- Consumes: `RequestResponse<T>` / `RequestResponseStatus`, `IExamService`, `IMinioStorageService` (`TVT.Core.MinioStorage`).
- Produces: `Task<RequestResponse<string>> ExportPdfAsync(...)`, `Task<RequestResponse<string>> ExportDocxAsync(...)`.

- [ ] **Step 1: Write the failing tests**

Create `exam_hub_api/ExamHub.Tests/ExportServiceTests.cs`:

```csharp
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using TVT.Core.Enums;
using TVT.Core.MinioStorage;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeExamServiceForExport : IExamService
{
    public Exam? WithQuestionsResult { get; set; }

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default) => Task.FromResult(WithQuestionsResult);
    public Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(int page, int pageSize, int? gradeLevelId = null, int? subjectId = null, ExamStatusEnum? status = null, string? keyword = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Exam> CreateAsync(Exam entity, IEnumerable<ExamQuestion> questions, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> PublishAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ArchiveAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<ExamHub.Core.DataTransferObjects.Exam.ExamAnalyticsResponse?> GetAnalyticsAsync(Guid examId, CancellationToken ct = default) => throw new NotSupportedException();
}

file sealed class FakeMinioStorageService : IMinioStorageService
{
    public bool UploadSucceeds { get; set; } = true;
    public string? UploadedUrl { get; set; } = "https://minio.local/exports/fake.pdf";

    public Task SetBucket(string bucketName) => throw new NotSupportedException();
    public Task RemoveBucket(string bucketName) => throw new NotSupportedException();
    public Task<(bool, string?)> UploadFileAsync(string filePath, string objectName, string contentType = "application/octet-stream") => throw new NotSupportedException();
    public Task<(bool, string?)> UploadStreamAsync(Stream stream, string objectName, string contentType = "application/octet-stream")
        => Task.FromResult((UploadSucceeds, UploadSucceeds ? UploadedUrl : null));
}

public class ExportServiceTests
{
    private static Exam SampleExam(Guid id) => new()
    {
        Id = id, Title = "Đề kiểm tra", SubjectId = 1, GradeLevelId = 1,
        Questions = [],
    };

    [Fact]
    public async Task ExportPdfAsync_ExamNotFound_ReturnsError()
    {
        var examService = new FakeExamServiceForExport { WithQuestionsResult = null };
        var service = new ExportService(examService, new FakeMinioStorageService());
        var examId = Guid.NewGuid();

        var result = await service.ExportPdfAsync(examId);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal($"Đề thi {examId} không tồn tại.", result.Message);
    }

    [Fact]
    public async Task ExportPdfAsync_UploadFails_ReturnsError()
    {
        var examId = Guid.NewGuid();
        var examService = new FakeExamServiceForExport { WithQuestionsResult = SampleExam(examId) };
        var storage = new FakeMinioStorageService { UploadSucceeds = false };
        var service = new ExportService(examService, storage);

        var result = await service.ExportPdfAsync(examId);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Tải file đề thi lên MinIO thất bại.", result.Message);
    }

    [Fact]
    public async Task ExportPdfAsync_ValidExam_ReturnsSuccessWithUrl()
    {
        var examId = Guid.NewGuid();
        var examService = new FakeExamServiceForExport { WithQuestionsResult = SampleExam(examId) };
        var storage = new FakeMinioStorageService { UploadedUrl = "https://minio.local/exports/abc.pdf" };
        var service = new ExportService(examService, storage);

        var result = await service.ExportPdfAsync(examId);

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal("https://minio.local/exports/abc.pdf", result.Data);
    }

    [Fact]
    public async Task ExportDocxAsync_ExamNotFound_ReturnsError()
    {
        var examService = new FakeExamServiceForExport { WithQuestionsResult = null };
        var service = new ExportService(examService, new FakeMinioStorageService());
        var examId = Guid.NewGuid();

        var result = await service.ExportDocxAsync(examId);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal($"Đề thi {examId} không tồn tại.", result.Message);
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail to compile**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter ExportServiceTests -v q` (from `exam_hub_api/`)
Expected: build error — `ExportPdfAsync`/`ExportDocxAsync` still return `Task<string>`.

- [ ] **Step 3: Update `IExportService`**

Replace `exam_hub_api/ExamHub.Core/Application/Services/IExportService.cs`:

```csharp
using TVT.Core;

namespace ExamHub.Core.Application.Services;

/// <summary>
/// Interface cho dịch vụ xuất đề thi ra PDF / Word.
/// </summary>
public interface IExportService
{
    /// <summary>Xuất đề thi ra file PDF (QuestPDF) và lưu lên MinIO. Trả về presigned URL.</summary>
    Task<RequestResponse<string>> ExportPdfAsync(Guid examId, CancellationToken ct = default);

    /// <summary>Xuất đề thi ra file Word (ClosedXML) và lưu lên MinIO. Trả về presigned URL.</summary>
    Task<RequestResponse<string>> ExportDocxAsync(Guid examId, CancellationToken ct = default);
}
```

- [ ] **Step 4: Update `ExportService`**

In `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExportService.cs`, add `using TVT.Core;` and replace lines 34-61:

```csharp
/// <inheritdoc/>
public async Task<RequestResponse<string>> ExportPdfAsync(Guid examId, CancellationToken ct = default)
{
    var (exam, error) = await LoadExamAsync(examId, ct);
    if (error is not null) return RequestResponse<string>.Error(error);
    var bytes = RenderPdf(exam!);
    return await UploadAsync(bytes, $"exports/{examId}.pdf", "application/pdf");
}

/// <inheritdoc/>
public async Task<RequestResponse<string>> ExportDocxAsync(Guid examId, CancellationToken ct = default)
{
    var (exam, error) = await LoadExamAsync(examId, ct);
    if (error is not null) return RequestResponse<string>.Error(error);
    var bytes = RenderDocx(exam!);
    return await UploadAsync(bytes, $"exports/{examId}.docx", DocxContentType);
}

private async Task<(Exam? Exam, string? Error)> LoadExamAsync(Guid examId, CancellationToken ct)
{
    var exam = await examService.GetWithQuestionsAsync(examId, ct);
    return exam is null ? (null, $"Đề thi {examId} không tồn tại.") : (exam, null);
}

private async Task<RequestResponse<string>> UploadAsync(byte[] bytes, string objectName, string contentType)
{
    using var ms = new MemoryStream(bytes);
    var (ok, url) = await storage.UploadStreamAsync(ms, objectName, contentType);
    if (!ok || string.IsNullOrEmpty(url))
        return RequestResponse<string>.Error("Tải file đề thi lên MinIO thất bại.");
    return RequestResponse<string>.Success("Xuất đề thi thành công!", url, 1);
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test ExamHub.Tests/ExamHub.Tests.csproj --filter ExportServiceTests -v q` (from `exam_hub_api/`)
Expected: PASS, 4/4.

- [ ] **Step 6: Update `ExamController.Export`**

In `exam_hub_api/ExamHub.API/Controllers/Exam/ExamController.cs`, replace lines 130-148:

```csharp
[HttpGet("{id:guid}/export")]
public async Task<ActionResult<RequestResponse<object>>> Export(
    Guid id, [FromQuery] string format, CancellationToken ct)
{
    var existing = await service.GetByIdAsync(id, ct);
    if (existing is null) return NotFound();

    var fmt = (format ?? "pdf").Trim().ToLowerInvariant();
    var result = fmt switch
    {
        "pdf"  => await exportService.ExportPdfAsync(id, ct),
        "docx" => await exportService.ExportDocxAsync(id, ct),
        _      => RequestResponse<string>.Error("Định dạng không hợp lệ. Chỉ hỗ trợ 'pdf' hoặc 'docx'.")
    };
    if (result.Status == TVT.Core.Enums.RequestResponseStatus.Error)
        return Ok(RequestResponse<object>.Error(result.Message!));

    return Ok(RequestResponse<object>.Success("Xuất đề thi thành công!", new { Url = result.Data, Format = fmt }, 1));
}
```

- [ ] **Step 7: Build and run the full backend test suite**

Run (from `exam_hub_api/`): `dotnet build ExamHub.API/ExamHub.API.csproj -v q` then `dotnet test ExamHub.Tests/ExamHub.Tests.csproj -v q`
Expected: build succeeds; all tests pass (78 + 4 = 82).

- [ ] **Step 8: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Application/Services/IExportService.cs \
        exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExportService.cs \
        exam_hub_api/ExamHub.API/Controllers/Exam/ExamController.cs \
        exam_hub_api/ExamHub.Tests/ExportServiceTests.cs
git commit -m "refactor(api): return RequestResponse.Error instead of throwing in ExportService"
```

---

### Task 6: Final verification

**Files:** none (verification only).

**Interfaces:** none.

- [ ] **Step 1: Full solution build**

Run (from `exam_hub_api/`): `dotnet build ExamHub.API/ExamHub.API.csproj -v q`
Expected: `0 Error(s)`. (If a background `dotnet run`/`dotnet watch` process is locking `ExamHub.API.dll`/`ExamHub.Core.dll` — as happened earlier in this session — stop it first, or ask the user to stop it, before rebuilding.)

- [ ] **Step 2: Full test suite**

Run (from `exam_hub_api/`): `dotnet test ExamHub.Tests/ExamHub.Tests.csproj -v q`
Expected: `Passed! - Failed: 0, Passed: 82, Skipped: 0, Total: 82`.

- [ ] **Step 3: Confirm the OpenAPI spec did not drift**

The controller action signatures (`ActionResult<RequestResponse<X>>>`) are unchanged for every action touched in this plan — only the internal service call changed. Run:

Run: `git status --short exam_hub_api/ExamHub.API/ExamHub.API.json` (from repo root)
Expected: no output (the build-time OpenAPI generator produces no diff). If it *does* show a diff, inspect it — it should only be if a doc comment changed, not a schema/type change; if it's a real schema change, something in this plan was executed differently than specified and needs to be reconciled with the spec before committing.

- [ ] **Step 4: Grep for any remaining out-of-scope throws to confirm nothing else was touched**

Run: `grep -rn "throw new InvalidOperationException" exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs exam_hub_api/ExamHub.Core/DependencyContainer.cs`
Expected: 3 matches (`ExamSubmissionService.cs` lines ~192, ~199; `DependencyContainer.cs` line ~59) — confirms the intentionally-excluded call sites are untouched.

- [ ] **Step 5: Manual smoke check (if a full local stack is available)**

Requires PostgreSQL/Redis/MongoDB/MinIO running (`docker compose up` from `exam_hub_api/`) and the frontend dev server. Exercise one path per converted service and confirm the UI now shows the real Vietnamese message instead of a false "success" toast or a mangled "Invalid operation: ..." message:
- Teacher: try to publish an `ExamSession` with an empty pool → expect toast "Kỳ thi chưa có đề trong pool." (not "Đã phát hành kỳ thi").
- Student: exhaust `MaxAttempts` on a session, then try "Vào thi" again → expect "Bạn đã hết lượt làm bài." (already partly worked before via the `!res.data` check, but now the message is exact, not "Invalid operation: ...").
- Admin: assign a teacher to a class/subject already assigned to someone else → expect "Môn học đã được phân công cho giáo viên khác trong lớp này." instead of a false "Phân công giáo viên thành công!" toast.

If no local stack is available, state that explicitly instead of claiming this step passed.

- [ ] **Step 6: Update the memory/observation trail (optional, if this session uses claude-mem)**

No code change — skip if not applicable.
