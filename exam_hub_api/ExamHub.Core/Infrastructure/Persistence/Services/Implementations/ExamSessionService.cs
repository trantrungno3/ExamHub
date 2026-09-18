using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TVT.Core;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho kỳ thi (exam sessions), gồm logic bốc/khoá đề.</summary>
public class ExamSessionService(IExamSessionRepository _repo, IExamRepository _examRepo) : IExamSessionService
{
    // ── Quản lý ─────────────────────────────────────────────────────────
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
        entity.DurationMinutes = req.DurationMinutes;
        entity.MaxAttempts = req.MaxAttempts;
        entity.PickMode = Enum.Parse<ExamSessionPickModeEnum>(req.PickMode);
        entity.ModifiedBy = by;
        entity.Modified = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        return RequestResponse<bool>.Success("Cập nhật kỳ thi thành công!", true, 1);
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
        var assignments = new List<AssignmentResponse>(s.Assignments.Count);
        foreach (var a in s.Assignments)
        {
            var count = await _repo.CountStudentsForAssignmentAsync(a, ct);
            assignments.Add(new AssignmentResponse(
                a.Id,
                a.CohortId,
                a.CohortClass?.Cohort?.Name ?? a.Cohort?.Name,
                a.CohortClassId,
                a.CohortClass?.ClassName,
                a.CohortClass?.Cohort?.School?.Name ?? a.Cohort?.School?.Name,
                count));
        }
        return new ExamSessionDetailResponse(
            s.Id, s.Title, s.Description, s.SubjectId, s.Subject?.Name,
            s.GradeLevelId, s.GradeLevel?.Name, ToMs(s.OpenAt), ToMs(s.CloseAt),
            s.DurationMinutes, s.MaxAttempts, s.PickMode.ToString(), s.Status.ToString().ToLower(),
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

    /// <inheritdoc/>
    public Task CloseAsync(Guid id, CancellationToken ct = default)
        => _repo.SetStatusAsync(id, ExamSessionStatusEnum.Closed, ct);

    // ── Pool đề ─────────────────────────────────────────────────────────
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

    /// <inheritdoc/>
    public Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct = default)
        => _repo.RemoveExamAsync(sessionId, examId, ct);

    // ── Assignment ──────────────────────────────────────────────────────
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

    /// <inheritdoc/>
    public Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
        => _repo.RemoveAssignmentAsync(assignmentId, ct);

    /// <inheritdoc/>
    public Task<ExamSessionAssignment?> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken ct = default)
        => _repo.GetAssignmentByIdAsync(assignmentId, ct);

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
                Availability(now, s.Status, s.OpenAt, s.CloseAt),
                s.DurationMinutes, s.MaxAttempts, used,
                inProgress?.Id, inProgress?.ExamId));
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<RequestResponse<IReadOnlyList<SessionPoolItemResponse>>> GetPoolForStudentAsync(
        Guid sessionId, Guid studentId, CancellationToken ct = default)
    {
        var session = await _repo.GetByIdAsync(sessionId, ct);
        if (session is null)
            return RequestResponse<IReadOnlyList<SessionPoolItemResponse>>.Error("Không tìm thấy kỳ thi.");

        var accessError = await StudentAccessErrorAsync(session, studentId, DateTime.UtcNow, ct);
        if (accessError is not null)
            return RequestResponse<IReadOnlyList<SessionPoolItemResponse>>.Error(accessError);

        var pool = await _repo.GetPoolExamsAsync(sessionId, ct);
        var submissions = await _repo.GetStudentSubmissionsAsync(sessionId, studentId, ct);
        IReadOnlyList<SessionPoolItemResponse> result = pool.Select(e =>
        {
            var sub = submissions.FirstOrDefault(x => x.ExamId == e.Id);
            var state = sub is null
                ? "notStarted"
                : sub.Status == SubmissionStatusEnum.InProgress ? "inProgress" : "completed";
            return new SessionPoolItemResponse(e.Id, e.Title, e.ExamCode, e.TotalScore, state, sub?.Id);
        }).ToList();
        return RequestResponse<IReadOnlyList<SessionPoolItemResponse>>.Success(
            "Lấy danh sách thành công!", result, result.Count);
    }

    /// <inheritdoc/>
    public async Task<RequestResponse<StartSessionResponse>> StartAsync(
        Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken ct = default)
    {
        var session = await _repo.GetByIdAsync(sessionId, ct);
        if (session is null) return RequestResponse<StartSessionResponse>.Error("Không tìm thấy kỳ thi.");
        var now = DateTime.UtcNow;
        var accessError = await StudentAccessErrorAsync(session, studentId, now, ct);
        if (accessError is not null)
            return RequestResponse<StartSessionResponse>.Error(accessError);

        // Đang có lượt dở → trả lại đúng đề đó (Tiếp tục)
        var inProgress = await _repo.GetInProgressAsync(sessionId, studentId, ct);
        if (inProgress is not null)
            return RequestResponse<StartSessionResponse>.Success(
                "Vào thi thành công!", new StartSessionResponse(
                    inProgress.Id, inProgress.ExamId, ToMs(session.DeadlineFor(inProgress.StartedAt)), session.DurationMinutes), 1);

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
        try
        {
            await _repo.CreateSubmissionAsync(submission, ct);
        }
        // Thua race với request start song song — unique index của DB là chốt cuối. Trả lượt của
        // người thắng để start trở nên idempotent; mọi lỗi DB khác vẫn nổi lên.
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            var winner = await _repo.GetInProgressAsync(sessionId, studentId, ct);
            if (winner is null)
                return RequestResponse<StartSessionResponse>.Error("Lượt làm bài đã được tạo. Vui lòng tải lại.");
            submission = winner;
            examId = winner.ExamId;
        }
        return RequestResponse<StartSessionResponse>.Success(
            "Vào thi thành công!", new StartSessionResponse(
                submission.Id, examId, ToMs(session.DeadlineFor(submission.StartedAt)), session.DurationMinutes), 1);
    }

    // ── Helpers ─────────────────────────────────────────────────────────
    private static long ToMs(DateTime dt)
        => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeSpan.Zero).ToUnixTimeMilliseconds();

    private async Task<string?> StudentAccessErrorAsync(
        ExamSession session, Guid studentId, DateTime now, CancellationToken ct)
    {
        if (session.Status == ExamSessionStatusEnum.Closed || now > session.CloseAt)
            return "Kỳ thi đã đóng.";
        if (session.Status != ExamSessionStatusEnum.Published)
            return "Kỳ thi chưa mở.";
        if (now < session.OpenAt)
            return "Kỳ thi chưa đến giờ mở.";
        if (!await _repo.IsStudentAssignedAsync(session.Id, studentId, ct))
            return "Bạn không được giao kỳ thi này.";
        return null;
    }

    private static string Availability(
        DateTime now, ExamSessionStatusEnum status, DateTime openAt, DateTime closeAt)
    {
        if (status == ExamSessionStatusEnum.Closed || now > closeAt) return "closed";
        if (status != ExamSessionStatusEnum.Published || now < openAt) return "upcoming";
        return "open";
    }
}
