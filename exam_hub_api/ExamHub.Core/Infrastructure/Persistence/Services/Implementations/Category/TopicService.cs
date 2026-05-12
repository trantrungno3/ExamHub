using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Topic</summary>
public class TopicService : ITopicService
{
    private readonly ITopicRepository _repo;
    public TopicService(ITopicRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<Topic>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => _repo.GetBySubjectAsync(subjectId, ct);

    public Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default)
        => _repo.GetRootTopicsAsync(subjectId, ct);

    public Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default)
        => _repo.GetChildrenAsync(parentId, ct);

    public Task<Topic?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<Topic> CreateAsync(Topic entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<Topic> UpdateAsync(Topic entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
