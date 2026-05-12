namespace ExamHub.Core.Domain.Interfaces;

/// <summary>
/// Category service interface — CRUD tái sử dụng cho các entity dạng danh mục.
/// Mirror của <see cref="ICategoryRepository{TEntity,TKey}"/> ở tầng service.
/// </summary>
/// <typeparam name="TEntity">Kiểu entity danh mục</typeparam>
/// <typeparam name="TKey">Kiểu khóa chính</typeparam>
public interface ICategoryService<TEntity, TKey> where TEntity : class
{
    /// <summary>Lấy toàn bộ danh sách</summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Lấy danh sách đang kích hoạt</summary>
    Task<IReadOnlyList<TEntity>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Lấy theo ID</summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

    /// <summary>Tạo mới</summary>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Cập nhật</summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>Xóa theo ID</summary>
    Task DeleteAsync(TKey id, CancellationToken ct = default);

    /// <summary>Bật/tắt kích hoạt</summary>
    Task<bool> SetActiveAsync(TKey id, bool isActive, CancellationToken ct = default);
}
