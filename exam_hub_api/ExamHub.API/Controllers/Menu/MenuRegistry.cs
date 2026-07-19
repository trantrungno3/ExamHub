namespace ExamHub.API.Controllers.Menu;

/// <summary>
/// Danh sách tất cả mục menu trong hệ thống cùng với danh sách role được phép thấy.
/// Đây là config tĩnh — không lưu DB.
/// </summary>
internal static class MenuRegistry
{
    /// <summary>Một mục menu. <paramref name="Group"/> null = mục/nhóm gốc; khác null = con của nhóm cha có Key tương ứng.
    /// Nhóm cha có <paramref name="Path"/> null.</summary>
    private record MenuItem(string Key, string Label, string? Path, string Icon, int Order, string[] Roles, string? Group = null);

    private static readonly MenuItem[] Items =
    [
        new("dashboard",     "Tổng quan",        "/app/dashboard",     "dashboard", 1, ["Admin", "Teacher", "Student"]),
        new("questions",     "Câu hỏi",           "/app/questions",     "question",  2, ["Admin", "Teacher"]),
        // Nhóm cha "Quản lý đề thi"
        new("exam-mgmt",     "Quản lý đề thi",    null,                 "template",  3, ["Admin", "Teacher"]),
        new("exams",         "Mẫu đề thi",        "/app/exams",         "template",  1, ["Admin", "Teacher"], Group: "exam-mgmt"),
        new("generate",      "Sinh đề thi",       "/app/generate",      "generate",  2, ["Admin", "Teacher"], Group: "exam-mgmt"),
        new("exam-list",     "Đề thi",            "/app/exam-list",     "exam",      3, ["Admin", "Teacher"], Group: "exam-mgmt"),
        new("exam-sessions", "Kỳ thi",            "/app/exam-sessions", "session",   4, ["Admin", "Teacher"], Group: "exam-mgmt"),
        new("schools",       "Quản lý trường",    "/app/schools",       "school",    6, ["Admin"]),
        new("users",         "Người dùng",        "/app/users",         "user",      7, ["Admin"]),
        new("category",      "Danh mục",          "/app/category",      "category",  8, ["Admin"]),
    ];

    /// <summary>Trả về cây menu mà ít nhất một trong <paramref name="userRoles"/> được phép thấy.
    /// Các mục có Group gộp thành Children của nhóm cha; nhóm cha chỉ hiện khi có ≥1 con hợp lệ.</summary>
    public static IReadOnlyList<MenuItemResponse> GetForRoles(IEnumerable<string> userRoles)
    {
        var roleSet = new HashSet<string>(userRoles, StringComparer.OrdinalIgnoreCase);
        var visible = Items.Where(item => item.Roles.Any(roleSet.Contains)).ToList();

        // Con theo nhóm cha
        var childrenByGroup = visible
            .Where(i => i.Group is not null)
            .GroupBy(i => i.Group!)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(i => i.Order)
                      .Select(i => new MenuItemResponse(i.Key, i.Label, i.Path, i.Icon, i.Order))
                      .ToList());

        var result = new List<MenuItemResponse>();
        foreach (var item in visible.Where(i => i.Group is null).OrderBy(i => i.Order))
        {
            if (item.Path is null)
            {
                // Nhóm cha: chỉ thêm nếu có con hợp lệ
                if (childrenByGroup.TryGetValue(item.Key, out var children) && children.Count > 0)
                    result.Add(new MenuItemResponse(item.Key, item.Label, null, item.Icon, item.Order, children));
            }
            else
            {
                result.Add(new MenuItemResponse(item.Key, item.Label, item.Path, item.Icon, item.Order));
            }
        }
        return result;
    }
}
