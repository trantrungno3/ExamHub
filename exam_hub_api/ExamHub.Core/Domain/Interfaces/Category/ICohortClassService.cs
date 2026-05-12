using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho CohortClass</summary>
public interface ICohortClassService
{
    /// <summary>Lấy danh sách lớp học theo khoá</summary>
    Task<IReadOnlyList<CohortClass>> GetByCohortAsync(int cohortId, CancellationToken ct = default);
    /// <summary>Lấy danh sách lớp học theo năm học</summary>
    Task<IReadOnlyList<CohortClass>> GetBySchoolYearAsync(string schoolYear, CancellationToken ct = default);
    /// <summary>Lấy theo ID</summary>
    Task<CohortClass?> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>Cập nhật giáo viên chủ nhiệm</summary>
    Task<bool> SetHomeroomTeacherAsync(int id, Guid? teacherId, CancellationToken ct = default);
}
