namespace ExamHub.API.Controllers.Menu;

/// <summary>Một mục trong menu điều hướng, đã được lọc theo quyền của người dùng hiện tại</summary>
public record MenuItemResponse(
    string Key,
    string Label,
    string Path,
    string Icon,
    int Order
);
