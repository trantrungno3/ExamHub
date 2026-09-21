namespace ExamHub.Core.DataTransferObjects.Common;

/// <summary>
/// Kết quả phân trang chuẩn cho mọi list endpoint. Tên trường `Total` khớp contract frontend đang
/// dùng (`{total, page, pageSize, items}`), đừng đổi thành TotalCount.
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize)
{
    /// <summary>Tổng số trang.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

    /// <summary>Tạo PagedResult từ danh sách đã phân trang.</summary>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int total, int page, int pageSize) =>
        new(items, total, page, pageSize);
}

/// <summary>Request phân trang chung.</summary>
public record PageRequest(int Page = 1, int PageSize = 20)
{
    /// <summary>Số bản ghi tối đa một trang được phép lấy.</summary>
    public const int MaxPageSize = 100;

    /// <summary>Offset cho DB query.</summary>
    public int Offset => (Page - 1) * PageSize;

    /// <summary>
    /// Clamp về khoảng hợp lệ. `page` dưới 1 cho offset âm (PostgreSQL ném lỗi thay vì trả trang
    /// đầu), còn `pageSize` không chặn trên thì một request quét cả bảng.
    /// </summary>
    public static (int Page, int PageSize) Normalize(int page, int pageSize) =>
        (Math.Max(1, page), Math.Clamp(pageSize, 1, MaxPageSize));

    /// <summary>Bản thân request với page/pageSize đã clamp.</summary>
    public PageRequest Normalized()
    {
        var (page, pageSize) = Normalize(Page, PageSize);
        return new PageRequest(page, pageSize);
    }
}
