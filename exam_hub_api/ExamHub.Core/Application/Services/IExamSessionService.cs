using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.Application.Services;

/// <summary>Service cho kỳ thi — quản lý (giáo viên/admin) và luồng làm bài (học sinh).</summary>
public interface IExamSessionService
{
    // ── Quản lý ─────────────────────────────────────────────────────────
    Task<Guid> CreateAsync(CreateExamSessionRequest req, string by, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateExamSessionRequest req, string by, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ExamSessionDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ExamSessionResponse> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? subjectId, int? gradeLevelId,
        ExamSessionStatusEnum? status, string? keyword, CancellationToken ct = default);
    Task PublishAsync(Guid id, CancellationToken ct = default);
    Task CloseAsync(Guid id, CancellationToken ct = default);

    // ── Pool đề ─────────────────────────────────────────────────────────
    Task SetExamsAsync(Guid sessionId, IReadOnlyList<Guid> examIds, string by, CancellationToken ct = default);
    Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct = default);

    // ── Assignment ──────────────────────────────────────────────────────
    Task<Guid> AddAssignmentAsync(Guid sessionId, CreateAssignmentRequest req, CancellationToken ct = default);
    Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct = default);

    // ── Phía học sinh ───────────────────────────────────────────────────
    Task<IReadOnlyList<MySessionResponse>> GetMySessionsAsync(Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyList<SessionPoolItemResponse>> GetPoolForStudentAsync(Guid sessionId, Guid studentId, CancellationToken ct = default);
    Task<StartSessionResponse> StartAsync(Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken ct = default);
}
