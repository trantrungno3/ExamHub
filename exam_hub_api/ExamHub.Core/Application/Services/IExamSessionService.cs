using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Enums;
using TVT.Core;

namespace ExamHub.Core.Application.Services;

/// <summary>Service cho kỳ thi — quản lý (giáo viên/admin) và luồng làm bài (học sinh).</summary>
public interface IExamSessionService
{
    // ── Quản lý ─────────────────────────────────────────────────────────
    /// <summary>Tạo kỳ thi mới (trạng thái Draft — chưa có đề/giao lớp).</summary>
    Task<RequestResponse<Guid>> CreateAsync(CreateExamSessionRequest req, string by, CancellationToken ct = default);

    /// <summary>Cập nhật cấu hình kỳ thi.</summary>
    Task<RequestResponse<bool>> UpdateAsync(Guid id, UpdateExamSessionRequest req, string by, CancellationToken ct = default);

    /// <summary>Xoá kỳ thi.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Chi tiết kỳ thi kèm pool đề và danh sách assignment.</summary>
    Task<ExamSessionDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default);

    /// <summary>Danh sách kỳ thi phân trang, lọc theo môn/lớp/trạng thái/từ khoá.</summary>
    Task<(IReadOnlyList<ExamSessionResponse> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? subjectId, int? gradeLevelId,
        ExamSessionStatusEnum? status, string? keyword, CancellationToken ct = default);

    /// <summary>
    /// Xuất bản kỳ thi (Draft → Published, mở cho học sinh làm bài). Yêu cầu: đã có ít nhất 1 đề
    /// trong pool, đã giao cho ít nhất 1 lớp/khoá, và CloseAt phải ở tương lai.
    /// </summary>
    Task<RequestResponse<bool>> PublishAsync(Guid id, CancellationToken ct = default);

    /// <summary>Đóng kỳ thi trước hạn (ngăn học sinh bắt đầu làm bài mới).</summary>
    Task CloseAsync(Guid id, CancellationToken ct = default);

    // ── Pool đề ─────────────────────────────────────────────────────────
    /// <summary>Gán danh sách đề (pool) mà học sinh có thể làm khi tham gia kỳ thi này.</summary>
    Task<RequestResponse<bool>> SetExamsAsync(Guid sessionId, IReadOnlyList<Guid> examIds, string by, CancellationToken ct = default);

    /// <summary>Gỡ một đề khỏi pool của kỳ thi.</summary>
    Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct = default);

    // ── Assignment ──────────────────────────────────────────────────────
    /// <summary>Giao kỳ thi cho một lớp/khoá cụ thể.</summary>
    Task<RequestResponse<Guid>> AddAssignmentAsync(Guid sessionId, CreateAssignmentRequest req, CancellationToken ct = default);

    /// <summary>Gỡ một assignment (thu hồi việc giao kỳ thi cho lớp/khoá đó).</summary>
    Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct = default);

    /// <summary>Chi tiết một assignment theo Id (dùng để kiểm tra quyền trước khi gỡ).</summary>
    Task<ExamHub.Core.Domain.Entities.ExamSessionAssignment?> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken ct = default);

    // ── Phía học sinh ───────────────────────────────────────────────────
    /// <summary>Danh sách kỳ thi mà học sinh này được giao (qua assignment lớp/khoá).</summary>
    Task<IReadOnlyList<MySessionResponse>> GetMySessionsAsync(Guid studentId, CancellationToken ct = default);

    /// <summary>Danh sách đề trong pool của kỳ thi mà học sinh có thể xem/chọn trước khi bắt đầu.</summary>
    Task<IReadOnlyList<SessionPoolItemResponse>> GetPoolForStudentAsync(Guid sessionId, Guid studentId, CancellationToken ct = default);

    /// <summary>
    /// Bắt đầu làm bài — kiểm tra kỳ thi đã Published, còn trong khung OpenAt/CloseAt, học sinh đã
    /// được giao. <paramref name="chosenExamId"/> chỉ dùng khi PickMode=StudentChoice; PickMode=Random
    /// thì hệ thống tự bốc đề trong pool, tham số này bị bỏ qua.
    /// </summary>
    Task<RequestResponse<StartSessionResponse>> StartAsync(Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken ct = default);
}
