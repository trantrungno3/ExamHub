using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho CohortMember</summary>
public interface ICohortMemberService
{
    /// <summary>Lấy danh sách học sinh theo khoá</summary>
    Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default);
    /// <summary>Lấy các khoá học của một học sinh</summary>
    Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Thêm học sinh vào khoá học</summary>
    Task<CohortMember> AddStudentAsync(CohortMember entity, CancellationToken ct = default);
    /// <summary>Xóa học sinh khỏi khoá học</summary>
    Task RemoveStudentAsync(Guid id, CancellationToken ct = default);
    /// <summary>Bật/tắt trạng thái học sinh trong khoá</summary>
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    /// <summary>Đổi lớp (section) của học sinh; validate thuộc dải lớp của khoá</summary>
    Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default);
}
