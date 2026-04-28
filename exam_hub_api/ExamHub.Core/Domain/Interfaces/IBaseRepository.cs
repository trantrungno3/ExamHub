using System.Linq.Expressions;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>
/// Base repository interface — CRUD cơ bản cho mọi entity
/// </summary>
/// <typeparam name="TEntity">Kiểu entity</typeparam>
/// <typeparam name="TKey">Kiểu khóa chính</typeparam>
public interface IBaseRepository<TEntity, TKey> where TEntity : class
{
    // ── Queries ─────────────────────────────────────────────────
    /// <summary>Lấy entity theo ID</summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

    /// <summary>Lấy toàn bộ danh sách</summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Lấy danh sách theo điều kiện</summary>
    Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    /// <summary>Lấy một entity theo điều kiện</summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    /// <summary>Kiểm tra tồn tại theo điều kiện</summary>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    /// <summary>Đếm số lượng</summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

    // ── Commands ────────────────────────────────────────────────
    /// <summary>Thêm mới entity</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Thêm mới nhiều entity</summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    /// <summary>Cập nhật entity</summary>
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Xóa entity</summary>
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Xóa theo ID</summary>
    Task DeleteByIdAsync(TKey id, CancellationToken ct = default);

    /// <summary>Lưu tất cả thay đổi vào database</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

