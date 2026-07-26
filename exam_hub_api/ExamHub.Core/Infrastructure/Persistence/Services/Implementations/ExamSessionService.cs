using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho kỳ thi (exam sessions), gồm logic bốc/khoá đề.</summary>
public class ExamSessionService(IExamSessionRepository _repo, IExamRepository _examRepo) : IExamSessionService
{
    // ── Quản lý ─────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task<Guid> CreateAsync(CreateExamSessionRequest req, string by, CancellationToken ct = default)
    {
        if (req.CloseAt.ToUniversalTime() <= req.OpenAt.ToUniversalTime())
            throw new InvalidOperationException("Thời điểm đóng phải sau thời điểm mở.");
        var entity = req.ToEntity();
        entity.CreatedBy = by;
        entity.ModifiedBy = by;
        entity.Created = DateTime.UtcNow;
        entity.Modified = DateTime.UtcNow;
        await _repo.AddAsync(entity, ct);
        return entity.Id;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Guid id, UpdateExamSessionRequest req, string by, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
        if (entity.Status == ExamSessionStatusEnum.Closed)
            throw new InvalidOperationException("Kỳ thi đã đóng, không thể sửa.");
        if (req.CloseAt.ToUniversalTime() <= req.OpenAt.ToUniversalTime())
            throw new InvalidOperationException("Thời điểm đóng phải sau thời điểm mở.");

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
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => _repo.DeleteAsync(id, ct);

    /// <inheritdoc/>
    public async Task<ExamSessionDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _repo.GetDetailAsync(id, ct);
        if (s is null) return null;
        var exams = s.Exams
            .Where(e => e.Exam is not null)
            .Select(e => new SessionExamResponse(e.ExamId, e.Exam!.Title, e.Exam.ExamCode, e.Exam.TotalScore))
            .ToList();
        var assignments = s.Assignments
            .Select(a => new AssignmentResponse(
                a.Id,
                a.CohortId,
                a.CohortClass?.Cohort?.Name ?? a.Cohort?.Name,
                a.CohortClassId,
                a.CohortClass?.ClassName,
                a.CohortClass?.Cohort?.School?.Name ?? a.Cohort?.School?.Name))
            .ToList();
        return new ExamSessionDetailResponse(
            s.Id, s.Title, s.Description, s.SubjectId, s.Subject?.Name,
            s.GradeLevelId, s.GradeLevel?.Name, ToMs(s.OpenAt), ToMs(s.CloseAt),
            s.MaxAttempts, s.PickMode.ToString(), s.Status.ToString().ToLower(),
            exams, assignments);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<ExamSessionResponse> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? subjectId, int? gradeLevelId,
        ExamSessionStatusEnum? status, string? keyword, CancellationToken ct = default)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, subjectId, gradeLevelId, status, keyword, ct);
        return (items.Select(ExamSessionResponse.FromEntity).ToList(), total);
    }

    /// <inheritdoc/>
    public async Task PublishAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _repo.GetDetailAsync(id, ct)
            ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
        if (s.Exams.Count == 0)
            throw new InvalidOperationException("Kỳ thi chưa có đề trong pool.");
        if (s.Assignments.Count == 0)
            throw new InvalidOperationException("Kỳ thi chưa được giao cho lớp/khoá nào.");
        if (s.CloseAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Thời điểm đóng phải ở tương lai.");
        await _repo.SetStatusAsync(id, ExamSessionStatusEnum.Published, ct);
    }

    /// <inheritdoc/>
    public Task CloseAsync(Guid id, CancellationToken ct = default)
        => _repo.SetStatusAsync(id, ExamSessionStatusEnum.Closed, ct);

    // ── Pool đề ─────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task SetExamsAsync(Guid sessionId, IReadOnlyList<Guid> examIds, string by, CancellationToken ct = default)
    {
        var session = await _repo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
        foreach (var examId in examIds.Distinct())
        {
            var exam = await _examRepo.GetByIdAsync(examId, ct)
                ?? throw new InvalidOperationException($"Không tìm thấy đề {examId}.");
            if (exam.Status != ExamStatusEnum.Published)
                throw new InvalidOperationException($"Đề '{exam.Title}' chưa phát hành.");
            if (exam.SubjectId != session.SubjectId || exam.GradeLevelId != session.GradeLevelId)
                throw new InvalidOperationException($"Đề '{exam.Title}' không cùng môn/cấp lớp với kỳ thi.");
        }
        await _repo.AddExamsAsync(sessionId, examIds, ct);
    }

    /// <inheritdoc/>
    public Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct = default)
        => _repo.RemoveExamAsync(sessionId, examId, ct);

    // ── Assignment ──────────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task<Guid> AddAssignmentAsync(Guid sessionId, CreateAssignmentRequest req, CancellationToken ct = default)
    {
        _ = await _repo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
        var hasCohort = req.CohortId is not null;
        var hasClass = req.CohortClassId is not null;
        if (hasCohort == hasClass)
            throw new InvalidOperationException("Chọn đúng một trong hai: khoá hoặc lớp.");
        var assignment = new ExamSessionAssignment
        {
            SessionId = sessionId,
            CohortId = req.CohortId,
            CohortClassId = req.CohortClassId
        };
        await _repo.AddAssignmentAsync(assignment, ct);
        return assignment.Id;
    }

    /// <inheritdoc/>
    public Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
        => _repo.RemoveAssignmentAsync(assignmentId, ct);

    // ── Phía học sinh ───────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task<IReadOnlyList<MySessionResponse>> GetMySessionsAsync(Guid studentId, CancellationToken ct = default)
    {
        var sessions = await _repo.GetAssignedToStudentAsync(studentId, ct);
        var now = DateTime.UtcNow;
        var result = new List<MySessionResponse>(sessions.Count);
        foreach (var s in sessions)
        {
            var used = await _repo.CountSubmittedAttemptsAsync(s.Id, studentId, ct);
            var inProgress = await _repo.GetInProgressAsync(s.Id, studentId, ct);
            result.Add(new MySessionResponse(
                s.Id, s.Title, s.Subject?.Name, s.GradeLevel?.Name,
                ToMs(s.OpenAt), ToMs(s.CloseAt), s.PickMode.ToString(),
                Availability(now, s.OpenAt, s.CloseAt),
                s.MaxAttempts, used,
                inProgress?.Id, inProgress?.ExamId));
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SessionPoolItemResponse>> GetPoolForStudentAsync(
        Guid sessionId, Guid studentId, CancellationToken ct = default)
    {
        var pool = await _repo.GetPoolExamsAsync(sessionId, ct);
        var submissions = await _repo.GetStudentSubmissionsAsync(sessionId, studentId, ct);
        return pool.Select(e =>
        {
            var sub = submissions.FirstOrDefault(x => x.ExamId == e.Id);
            var state = sub is null
                ? "notStarted"
                : sub.Status == SubmissionStatusEnum.InProgress ? "inProgress" : "completed";
            return new SessionPoolItemResponse(e.Id, e.Title, e.ExamCode, e.TotalScore, state, sub?.Id);
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<StartSessionResponse> StartAsync(
        Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken ct = default)
    {
        var session = await _repo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
        if (session.Status != ExamSessionStatusEnum.Published)
            throw new InvalidOperationException("Kỳ thi chưa mở.");
        var now = DateTime.UtcNow;
        if (now < session.OpenAt) throw new InvalidOperationException("Kỳ thi chưa đến giờ mở.");
        if (now > session.CloseAt) throw new InvalidOperationException("Kỳ thi đã đóng.");
        if (!await _repo.IsStudentAssignedAsync(sessionId, studentId, ct))
            throw new InvalidOperationException("Bạn không được giao kỳ thi này.");

        // Đang có lượt dở → trả lại đúng đề đó (Tiếp tục)
        var inProgress = await _repo.GetInProgressAsync(sessionId, studentId, ct);
        if (inProgress is not null)
            return new StartSessionResponse(inProgress.Id, inProgress.ExamId);

        var used = await _repo.CountSubmittedAttemptsAsync(sessionId, studentId, ct);
        if (used >= session.MaxAttempts)
            throw new InvalidOperationException("Bạn đã hết lượt làm bài.");

        var pool = await _repo.GetPoolExamsAsync(sessionId, ct);
        if (pool.Count == 0) throw new InvalidOperationException("Kỳ thi chưa có đề.");

        Guid examId;
        if (session.PickMode == ExamSessionPickModeEnum.StudentChoice)
        {
            if (chosenExamId is null) throw new InvalidOperationException("Vui lòng chọn đề.");
            if (pool.All(e => e.Id != chosenExamId.Value))
                throw new InvalidOperationException("Đề không thuộc kỳ thi.");
            var done = await _repo.GetStudentSubmissionsAsync(sessionId, studentId, ct);
            if (done.Any(s => s.ExamId == chosenExamId.Value && s.Status != SubmissionStatusEnum.InProgress))
                throw new InvalidOperationException("Bạn đã làm đề này rồi.");
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
            ModifiedBy = by
        };
        await _repo.CreateSubmissionAsync(submission, ct);
        return new StartSessionResponse(submission.Id, examId);
    }

    // ── Helpers ─────────────────────────────────────────────────────────
    private static long ToMs(DateTime dt)
        => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeSpan.Zero).ToUnixTimeMilliseconds();

    private static string Availability(DateTime now, DateTime openAt, DateTime closeAt)
        => now < openAt ? "upcoming" : now > closeAt ? "closed" : "open";
}
