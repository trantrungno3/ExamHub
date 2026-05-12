using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho Topic</summary>
public interface ITopicRepository : ICategoryRepository<Topic, int>
{
    /// <summary>Lấy danh sách chủ đề theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default);

    /// <summary>Lấy chủ đề con theo chủ đề cha</summary>
    Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default);

    /// <summary>Lấy chủ đề gốc (không có cha) theo môn học</summary>
    Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default);
}
