using System.Security.Claims;
using ExamHub.Core.Application.Services;
using TVT.Core.Extensions;
using TVT.Core.Utils;

namespace ExamHub.API.Authorization;

/// <summary>Thông tin người dùng hiện tại được trích xuất từ claim trong JWT token</summary>
public sealed class CurrentUserInfo
{
    /// <summary>Khởi tạo thông tin người dùng từ claim trong token</summary>
    /// <param name="user">Principal đã xác thực (từ <c>HttpContext.User</c>); null thì mọi thuộc tính giữ giá trị mặc định.</param>
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
        => user.FindAll(claimType)
            .Select(c => int.TryParse(c.Value, out var value) ? value : (int?)null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

    /// <summary>ID người dùng</summary>
    public Guid? UserId { get; set; }

    /// <summary>Tên đăng nhập</summary>
    public string? UserName { get; set; }

    /// <summary>Tên hiển thị</summary>
    public string? DisplayName { get; set; }

    /// <summary>Danh sách vai trò</summary>
    public IReadOnlyList<string>? Roles { get; set; }

    /// <summary>Tag của người dùng (dùng để gán CreatedBy/UpdatedBy trên các bản ghi).</summary>
    public string? Tag { get; set; }

    /// <summary>Id các trường (Teacher: nhiều trường; Student: trường của khoá đang học)</summary>
    public IReadOnlyList<int> SchoolIds { get; set; } = [];

    /// <summary>Id các lớp (Teacher: dạy + chủ nhiệm; Student: lớp hiện tại)</summary>
    public IReadOnlyList<int> CohortClassIds { get; set; } = [];

    /// <summary>Id các môn phụ trách (chỉ Teacher)</summary>
    public IReadOnlyList<int> SubjectIds { get; set; } = [];
}