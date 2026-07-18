namespace ExamHub.API.Controllers.Menu;

/// <summary>
/// Danh sách tất cả mục menu trong hệ thống cùng với danh sách role được phép thấy.
/// Đây là config tĩnh — không lưu DB.
/// </summary>
internal static class MenuRegistry
{
    private record MenuItem(string Key, string Label, string Path, string Icon, int Order, string[] Roles);

    private static readonly MenuItem[] Items =
    [
        new("dashboard",  "Tổng quan",       "/app/dashboard",  "dashboard",      1, ["Admin", "Teacher", "Student"]),
        new("questions",  "Câu hỏi",          "/app/questions",  "question",       2, ["Admin", "Teacher"]),
        new("exams",      "Mẫu đề thi",       "/app/exams",      "template",       3, ["Admin", "Teacher"]),
        new("generate",   "Sinh đề thi",      "/app/generate",   "generate",       4, ["Admin", "Teacher"]),
        new("exam-list",  "Đề thi",           "/app/exam-list",  "exam",           5, ["Admin", "Teacher"]),
        new("schools",    "Quản lý trường",   "/app/schools",    "school",         6, ["Admin"]),
        new("users",      "Người dùng",       "/app/users",      "user",           7, ["Admin"]),
        new("category",   "Danh mục",         "/app/category",   "category",       8, ["Admin"]),
    ];

    /// <summary>Trả về các mục menu mà ít nhất một trong <paramref name="userRoles"/> được phép thấy.</summary>
    public static IReadOnlyList<MenuItemResponse> GetForRoles(IEnumerable<string> userRoles)
    {
        var roleSet = new HashSet<string>(userRoles, StringComparer.OrdinalIgnoreCase);
        return Items
            .Where(item => item.Roles.Any(roleSet.Contains))
            .OrderBy(item => item.Order)
            .Select(item => new MenuItemResponse(item.Key, item.Label, item.Path, item.Icon, item.Order))
            .ToList();
    }
}
