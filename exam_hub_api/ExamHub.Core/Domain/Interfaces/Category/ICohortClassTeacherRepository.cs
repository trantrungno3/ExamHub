using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Repository cho phân công GV giảng dạy cho lớp (CohortClassTeacher)</summary>
public interface ICohortClassTeacherRepository : IBaseRepository<CohortClassTeacher, int>
{
    /// <summary>
    /// Danh sách Id giáo viên hợp lệ để phân công môn cho lớp:
    /// là thành viên trường (role Teacher, đang hoạt động) của trường sở hữu khoá
    /// VÀ có môn đó trong teacher_subjects.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default);
}
