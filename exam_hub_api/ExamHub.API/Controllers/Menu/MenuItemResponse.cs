namespace ExamHub.API.Controllers.Menu;

/// <summary>Một mục trong menu điều hướng, đã được lọc theo quyền của người dùng hiện tại.
/// Nhóm cha có <see cref="Path"/> null và <see cref="Children"/> chứa các mục con.</summary>
/// <param name="Key">Mã định danh duy nhất của mục menu.</param>
/// <param name="Label">Nhãn hiển thị.</param>
/// <param name="Path">Đường dẫn route; null nếu đây là nhóm cha không có trang riêng.</param>
/// <param name="Icon">Tên icon hiển thị.</param>
/// <param name="Order">Thứ tự hiển thị trong menu/nhóm.</param>
/// <param name="Children">Danh sách mục con (nếu là nhóm cha); null nếu là mục lá.</param>
public record MenuItemResponse(
    string Key,
    string Label,
    string? Path,
    string Icon,
    int Order,
    IReadOnlyList<MenuItemResponse>? Children = null
);
