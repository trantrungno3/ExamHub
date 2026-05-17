using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using TVT.Core.Db.Redis;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho DifficultyLevel</summary>
public class DifficultyLevelService(IDifficultyLevelRepository repo, IRedisService cache)
    : IDifficultyLevelService
{
    private const string AllKey    = "category:difficulty-levels:all";
    private const string ActiveKey = "category:difficulty-levels:active";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public Task<IReadOnlyList<DifficultyLevel>> GetAllAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(AllKey, () => repo.GetAllAsync(ct), Ttl, ct)!;

    public Task<IReadOnlyList<DifficultyLevel>> GetActiveAsync(CancellationToken ct = default)
        => cache.GetOrSetAsync(ActiveKey, () => repo.GetActiveAsync(ct), Ttl, ct)!;

    public Task<DifficultyLevel?> GetByIdAsync(int id, CancellationToken ct = default)
        => repo.GetByIdAsync(id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => repo.ExistsAsync(e => e.Id == id, ct);

    public Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
        => repo.GetByCodeAsync(code, ct);

    public async Task<DifficultyLevel> CreateAsync(DifficultyLevel entity, CancellationToken ct = default)
    {
        var result = await repo.AddAsync(entity, ct);
        await InvalidateCacheAsync(ct);
        return result;
    }

    public async Task<DifficultyLevel> UpdateAsync(DifficultyLevel entity, CancellationToken ct = default)
    {
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
