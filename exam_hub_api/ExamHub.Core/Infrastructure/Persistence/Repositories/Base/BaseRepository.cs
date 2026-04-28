using System.Linq.Expressions;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Base;

/// <summary>
/// Triển khai base repository dùng EF Core
/// </summary>
/// <typeparam name="TEntity">Kiểu entity</typeparam>
/// <typeparam name="TKey">Kiểu khóa chính</typeparam>
public class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>DbContext được inject</summary>
    protected readonly AppDbContext Db;

    /// <summary>DbSet tương ứng</summary>
    protected readonly DbSet<TEntity> Set;

    /// <inheritdoc cref="BaseRepository{TEntity,TKey}"/>
    public BaseRepository(AppDbContext db)
    {
        Db  = db;
        Set = db.Set<TEntity>();
    }

    // ── Queries ─────────────────────────────────────────────────
    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => await Set.FindAsync([id], ct);

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await Set.AsNoTracking().Where(predicate).ToListAsync(ct);

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(predicate, ct);

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await Set.AnyAsync(predicate, ct);

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
        => predicate is null
            ? await Set.CountAsync(ct)
            : await Set.CountAsync(predicate, ct);

    // ── Commands ────────────────────────────────────────────────
    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        await Db.SaveChangesAsync(ct);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        await Set.AddRangeAsync(entities, ct);
        await Db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        Set.Update(entity);
        await Db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        Set.Remove(entity);
        await Db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task DeleteByIdAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' not found.");
        await DeleteAsync(entity, ct);
    }

    /// <inheritdoc />
    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Db.SaveChangesAsync(ct);
}

