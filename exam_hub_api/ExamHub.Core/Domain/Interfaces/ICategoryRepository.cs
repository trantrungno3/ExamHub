namespace ExamHub.Core.Domain.Interfaces;

/// <summary>
/// Category repository interface — CRUD đầy đủ cho các entity dạng danh mục
/// (có Name, Code, IsActive, Created, Modified)
/// </summary>
/// <typeparam name="TEntity">Kiểu entity danh mục</typeparam>
/// <typeparam name="TKey">Kiểu khóa chính</typeparam>
public interface ICategoryRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>Lấy danh sách đang kích hoạt</summary>
    Task<IReadOnlyList<TEntity>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Tìm kiếm theo tên (không phân biệt hoa thường)</summary>
    Task<IReadOnlyList<TEntity>> SearchByNameAsync(string keyword, CancellationToken ct = default);

    /// <summary>Bật/tắt trạng thái kích hoạt</summary>
    Task<bool> SetActiveAsync(TKey id, bool isActive, CancellationToken ct = default);
}

