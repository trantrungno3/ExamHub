using ExamHub.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Base;

/// <summary>
/// Triển khai category repository — thêm các thao tác danh mục phổ biến
/// (IsActive, SearchByName, SetActive)
/// </summary>
/// <typeparam name="TEntity">Kiểu entity danh mục (phải có IsActive, Name)</typeparam>
/// <typeparam name="TKey">Kiểu khóa chính</typeparam>
public abstract class CategoryRepository<TEntity, TKey>
    : BaseRepository<TEntity, TKey>, ICategoryRepository<TEntity, TKey>
    where TEntity : class
{
    /// <inheritdoc cref="CategoryRepository{TEntity,TKey}"/>
    protected CategoryRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<TEntity>> GetActiveAsync(CancellationToken ct = default);

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<TEntity>> SearchByNameAsync(string keyword, CancellationToken ct = default);

    /// <inheritdoc />
    public abstract Task<bool> SetActiveAsync(TKey id, bool isActive, CancellationToken ct = default);
}

