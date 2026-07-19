namespace ExamHub.API.Controllers.Menu;

/// <summary>Một mục trong menu điều hướng, đã được lọc theo quyền của người dùng hiện tại.
/// Nhóm cha có <see cref="Path"/> null và <see cref="Children"/> chứa các mục con.</summary>
public record MenuItemResponse(
    string Key,
    string Label,
    string? Path,
    string Icon,
    int Order,
    IReadOnlyList<MenuItemResponse>? Children = null
);
