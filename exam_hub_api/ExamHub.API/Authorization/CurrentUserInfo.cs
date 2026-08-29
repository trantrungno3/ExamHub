using System.Security.Claims;
using ExamHub.Core.Application.Services;
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
        SchoolIds = ParseIntClaims(user, TokenClaimTypes.SchoolId);
        CohortClassIds = ParseIntClaims(user, TokenClaimTypes.CohortClassId);
        SubjectIds = ParseIntClaims(user, TokenClaimTypes.SubjectId);
    }

    private static IReadOnlyList<int> ParseIntClaims(ClaimsPrincipal user, string claimType)
        => user.FindAll(claimType).Select(c => int.Parse(c.Value)).ToList();

    /// <summary>ID người dùng</summary>
    public Guid? UserId { get; set; }

    /// <summary>Tên đăng nhập</summary>
    public string? UserName { get; set; }

    /// <summary>Tên hiển thị</summary>
    public string? DisplayName { get; set; }

    /// <summary>Danh sách vai trò</summary>
    public IReadOnlyList<string>? Roles { get; set; }

    public string? Tag { get; set; }

    /// <summary>Id các trường (Teacher: nhiều trường; Student: trường của khoá đang học)</summary>
    public IReadOnlyList<int> SchoolIds { get; set; } = [];

    /// <summary>Id các lớp (Teacher: dạy + chủ nhiệm; Student: lớp hiện tại)</summary>
    public IReadOnlyList<int> CohortClassIds { get; set; } = [];

    /// <summary>Id các môn phụ trách (chỉ Teacher)</summary>
    public IReadOnlyList<int> SubjectIds { get; set; } = [];
}