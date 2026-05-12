using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho Subject</summary>
public interface ISubjectService : ICategoryService<Subject, int>
{
    /// <summary>Lấy theo lớp học</summary>
    Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default);

    /// <summary>Lấy kèm chủ đề</summary>
    Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default);
}
