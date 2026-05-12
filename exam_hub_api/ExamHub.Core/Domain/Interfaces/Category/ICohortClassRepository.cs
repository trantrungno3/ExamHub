using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho CohortClass</summary>
public interface ICohortClassRepository : IBaseRepository<CohortClass, int>
{
    /// <summary>Lấy danh sách lớp học theo khoá</summary>
    Task<IReadOnlyList<CohortClass>> GetByCohortAsync(int cohortId, CancellationToken ct = default);

    /// <summary>Lấy danh sách lớp học theo năm học</summary>
    Task<IReadOnlyList<CohortClass>> GetBySchoolYearAsync(string schoolYear, CancellationToken ct = default);

    /// <summary>Cập nhật giáo viên chủ nhiệm</summary>
    Task<bool> SetHomeroomTeacherAsync(int id, Guid? teacherId, CancellationToken ct = default);
}
