using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho GradeLevel</summary>
public interface IGradeLevelService : ICategoryService<GradeLevel, int>
{
    /// <summary>Lấy kèm môn học</summary>
    Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default);
}
