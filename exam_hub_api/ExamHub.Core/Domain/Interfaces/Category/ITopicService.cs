using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho Topic</summary>
public interface ITopicService : ICategoryService<Topic, int>
{
    /// <summary>Lấy theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);

    /// <summary>Lấy chủ đề gốc theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default);

    /// <summary>Lấy chủ đề con</summary>
    Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default);
}
