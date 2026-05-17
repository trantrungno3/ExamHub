using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using TVT.Core.Db.Redis;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Subject</summary>
public class SubjectService(ISubjectRepository repo, IRedisService cache)
    : ISubjectService
{
    private const string AllKey    = "category:subjects:all";
    private const string ActiveKey = "category:subjects:active";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(AllKey, () => repo.GetAllAsync(ct), Ttl, ct)!;

    public Task<IReadOnlyList<Subject>> GetActiveAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(ActiveKey, () => repo.GetActiveAsync(ct), Ttl, ct)!;

    public Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => repo.GetByGradeLevelAsync(gradeLevelId, ct);

    public Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default)
        => repo.GetByIdAsync(id, ct);

    public Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default)
        => repo.GetWithTopicsAsync(id, ct);

    public async Task<Subject> CreateAsync(Subject entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        var result = await repo.AddAsync(entity, ct);
        await InvalidateCacheAsync(ct);
        return result;
    }

    public async Task<Subject> UpdateAsync(Subject entity, CancellationToken ct = default)
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
