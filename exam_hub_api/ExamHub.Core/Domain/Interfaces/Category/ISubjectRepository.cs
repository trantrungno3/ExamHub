using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho Subject</summary>
public interface ISubjectRepository : ICategoryRepository<Subject, int>
{
    /// <summary>Lấy danh sách môn học theo lớp</summary>
    Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default);

    /// <summary>Lấy môn học kèm danh sách chủ đề</summary>
    Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default);
}
