namespace ExamHub.Core.DataTransferObjects.Common;

/// <summary>Kết quả phân trang chuẩn cho mọi list endpoint.</summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>Tổng số trang.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>Tạo PagedResult từ danh sách đã phân trang.</summary>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize) =>
        new(items, totalCount, page, pageSize);
}

/// <summary>Request phân trang chung.</summary>
public record PageRequest(int Page = 1, int PageSize = 20)
{
    /// <summary>Offset cho DB query.</summary>
    public int Offset => (Page - 1) * PageSize;
}
