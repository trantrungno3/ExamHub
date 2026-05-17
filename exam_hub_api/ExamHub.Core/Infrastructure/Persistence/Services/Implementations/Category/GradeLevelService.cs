using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using TVT.Core.Db.Redis;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho GradeLevel</summary>
public class GradeLevelService(IGradeLevelRepository repo, IRedisService cache)
    : IGradeLevelService
{
    private const string AllKey    = "category:grade-levels:all";
    private const string ActiveKey = "category:grade-levels:active";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public Task<IReadOnlyList<GradeLevel>> GetAllAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(AllKey, () => repo.GetAllAsync(ct), Ttl, ct)!;

    public Task<IReadOnlyList<GradeLevel>> GetActiveAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(ActiveKey, () => repo.GetActiveAsync(ct), Ttl, ct)!;

    public Task<GradeLevel?> GetByIdAsync(int id, CancellationToken ct = default)
        => repo.GetByIdAsync(id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => repo.ExistsAsync(e => e.Id == id, ct);

    public Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default)
        => repo.GetWithSubjectsAsync(id, ct);

    public async Task<GradeLevel> CreateAsync(GradeLevel entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        var result = await repo.AddAsync(entity, ct);
        await InvalidateCacheAsync(ct);
        return result;
    }

    public async Task<GradeLevel> UpdateAsync(GradeLevel entity, CancellationToken ct = default)
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
