using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho Cohort</summary>
public interface ICohortService : ICategoryService<Cohort, int>
{
    /// <summary>Lấy danh sách khoá học theo trường</summary>
    Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default);

    /// <summary>Lấy khoá học kèm danh sách lớp học</summary>
    Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default);

    /// <summary>Lấy khoá học kèm danh sách học sinh</summary>
    Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default);
}
