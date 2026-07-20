using System.Security.Claims;
using TVT.Core.Extensions;
using TVT.Core.Utils;

namespace ExamHub.API.Authorization;

/// <summary>Thông tin người dùng hiện tại được trích xuất từ claim trong JWT token</summary>
public sealed class CurrentUserInfo
{
    /// <summary>Khởi tạo thông tin người dùng từ claim trong token</summary>
    public CurrentUserInfo(ClaimsPrincipal? user)
    {
        if (user == null) return;
        UserId = user.GetUserid().ToNullableGuid();
        UserName = user.GetUserName();
        DisplayName = user.GetDisplayName();
        Roles = user.GetRoles();
        Tag = user.GetTag();
    }

    /// <summary>ID người dùng</summary>
    public Guid? UserId { get; set; }

    /// <summary>Tên đăng nhập</summary>
    public string? UserName { get; set; }

    /// <summary>Tên hiển thị</summary>
    public string? DisplayName { get; set; }

    /// <summary>Danh sách vai trò</summary>
    public IReadOnlyList<string>? Roles { get; set; }

    public string? Tag { get; set; }
}