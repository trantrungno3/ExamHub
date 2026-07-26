using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho CohortMember</summary>
public interface ICohortMemberRepository : IBaseRepository<CohortMember, Guid>
{
    /// <summary>Lấy danh sách học sinh theo khoá</summary>
    Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default);

    /// <summary>Lấy các khoá học của một học sinh</summary>
    Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default);

    /// <summary>Lấy bản ghi theo khoá + học sinh</summary>
    Task<CohortMember?> GetByCohortAndStudentAsync(int cohortId, Guid studentId, CancellationToken ct = default);

    /// <summary>Bật/tắt trạng thái học sinh trong khoá</summary>
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);

    /// <summary>Đổi lớp (section) của học sinh trong khoá</summary>
    Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default);
}
