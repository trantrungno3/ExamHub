using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho Cohort</summary>
public interface ICohortRepository : IBaseRepository<Cohort, int>
{
    /// <summary>Lấy danh sách khoá học đang hoạt động</summary>
    Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Lấy danh sách khoá học theo trường</summary>
    Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default);

    /// <summary>Lấy khoá học kèm danh sách lớp học</summary>
    Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default);

    /// <summary>Lấy khoá học kèm danh sách học sinh</summary>
    Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default);

    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}
