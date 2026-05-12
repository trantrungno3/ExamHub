using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho GradeLevel</summary>
public interface IGradeLevelRepository : ICategoryRepository<GradeLevel, int>
{
    /// <summary>Lấy GradeLevel kèm danh sách Subject</summary>
    Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default);
}
