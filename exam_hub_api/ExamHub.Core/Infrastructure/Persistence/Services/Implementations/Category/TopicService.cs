using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using TVT.Core.Db.Redis;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Topic</summary>
public class TopicService(ITopicRepository repo, IRedisService cache)
    : ITopicService
{
    private const string AllKey    = "category:topics:all";
    private const string ActiveKey = "category:topics:active";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(AllKey, () => repo.GetAllAsync(ct), Ttl, ct)!;

    public Task<IReadOnlyList<Topic>> GetActiveAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(ActiveKey, () => repo.GetActiveAsync(ct), Ttl, ct)!;

    public Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => repo.GetBySubjectAsync(subjectId, ct);

    public Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default)
        => repo.GetRootTopicsAsync(subjectId, ct);

    public Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default)
        => repo.GetChildrenAsync(parentId, ct);

    public Task<Topic?> GetByIdAsync(int id, CancellationToken ct = default)
        => repo.GetByIdAsync(id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => repo.ExistsAsync(e => e.Id == id, ct);

    public async Task<Topic> CreateAsync(Topic entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        var result = await repo.AddAsync(entity, ct);
        await InvalidateCacheAsync(ct);
        return result;
    }

    public async Task<Topic> UpdateAsync(Topic entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(entity, ct);
        await InvalidateCacheAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await repo.DeleteByIdAsync(id, ct);
        await InvalidateCacheAsync(ct);
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var result = await repo.SetActiveAsync(id, isActive, ct);
        await InvalidateCacheAsync(ct);
        return result;
    }

    private Task InvalidateCacheAsync(CancellationToken ct) =>
        Task.WhenAll(cache.RemoveAsync(AllKey, ct), cache.RemoveAsync(ActiveKey, ct));
}
