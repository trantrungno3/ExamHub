using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service cho phân công GV giảng dạy cho lớp</summary>
public interface ICohortClassTeacherService
{
    /// <summary>Danh sách phân công của một lớp</summary>
    Task<IReadOnlyList<CohortClassTeacher>> GetByClassAsync(int cohortClassId, CancellationToken ct = default);

    /// <summary>Danh sách Id GV hợp lệ để phân công môn cho lớp</summary>
    Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default);

    /// <summary>
    /// Phân công GV dạy môn cho lớp. Validate + kiểm ràng buộc (GV hợp lệ, không trùng môn/lớp)
    /// trước khi ghi DB; ném InvalidOperationException với thông báo nếu vi phạm.
    /// </summary>
    Task<CohortClassTeacher> AssignAsync(int cohortClassId, int subjectId, Guid teacherId, CancellationToken ct = default);

    /// <summary>Xoá một phân công theo Id (ném KeyNotFoundException nếu không tồn tại)</summary>
    Task RemoveAsync(int id, CancellationToken ct = default);
}
