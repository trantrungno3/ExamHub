using System.ComponentModel.DataAnnotations;
using TVT.Core.Extensions;
using TVT.Core.IdentityUser.PostgreSql.Models;
using TVT.Core.Utils;

namespace ExamHub.Core.DataTransferObjects.User;

/// <summary>Response DTO thông tin người dùng</summary>
public record UserResponse
{
    /// <summary>ID người dùng</summary>
    public Guid Id { get; init; }

    /// <summary>Tên đăng nhập</summary>
    public string? UserName { get; init; }

    /// <summary>Tên hiển thị</summary>
    public string DisplayName { get; init; } = "";

    /// <summary>Email</summary>
    public string? Email { get; init; }

    /// <summary>Số điện thoại</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>Giới tính (false = Nam, true = Nữ)</summary>
    public bool Sex { get; init; }

    /// <summary>Ảnh đại diện</summary>
    public string? Avartar { get; init; }

    /// <summary>Địa chỉ</summary>
    public string? Address { get; init; }

    /// <summary>Mô tả</summary>
    public string? Description { get; init; }

    /// <summary>Danh sách vai trò</summary>
    public string[] Roles { get; init; } = [];

    /// <summary>Tài khoản đang bị khóa</summary>
    public bool LockoutEnabled { get; init; }

    /// <summary>Tài khoản đã bị xóa</summary>
    public bool IsDeleted { get; init; }

    public static UserResponse FromEntity(UserAdmin u) => new()
    {
        Id             = u.Id,
        UserName       = u.UserName,
        DisplayName    = u.DisplayName,
        Email          = u.GetEmailName(),
        PhoneNumber    = u.PhoneNumber,
        Sex            = u.Sex,
        Avartar        = u.Avartar,
        Address        = u.Address,
        Description    = u.Description,
        Roles          = u.Roles,
        LockoutEnabled = u.LockoutEnabled,
        IsDeleted      = u.Deleted.HasValue,
    };
}

/// <summary>Request tạo người dùng mới</summary>
public record CreateUserRequest
{
    /// <summary>Tên đăng nhập</summary>
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string UserName { get; init; }

    /// <summary>Mật khẩu</summary>
    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string Password { get; init; }

    /// <summary>Tên hiển thị</summary>
    [Display(Name = "Tên hiển thị")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string DisplayName { get; init; }

    /// <summary>Email</summary>
    public string? Email { get; init; }

    /// <summary>Số điện thoại</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>Giới tính (false = Nam, true = Nữ)</summary>
    public bool Sex { get; init; }

    public UserAdmin ToEntity() => new()
    {
        UserName           = UserName,
        NormalizedUserName = UserName.ToUpperNormalize(),
        DisplayName        = DisplayName,
        PhoneNumber        = PhoneNumber,
        Sex                = Sex,
    };
}

/// <summary>Request cập nhật thông tin người dùng</summary>
public record UpdateUserRequest
{
    /// <summary>Tên hiển thị</summary>
    [Display(Name = "Tên hiển thị")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string DisplayName { get; init; }

    /// <summary>Email</summary>
    public string? Email { get; init; }

    /// <summary>Số điện thoại</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>Giới tính (false = Nam, true = Nữ)</summary>
    public bool Sex { get; init; }

    /// <summary>Ảnh đại diện</summary>
    public string? Avartar { get; init; }

    /// <summary>Địa chỉ</summary>
    public string? Address { get; init; }

    /// <summary>Mô tả</summary>
    public string? Description { get; init; }
}

/// <summary>Request đặt lại toàn bộ roles</summary>
public record SetRolesRequest
{
    /// <summary>Danh sách vai trò mới</summary>
    [Required]
    public required string[] Roles { get; init; }
}

/// <summary>Request đặt lại mật khẩu</summary>
public record ResetPasswordRequest
{
    /// <summary>Mật khẩu mới</summary>
    [Display(Name = "Mật khẩu mới")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string NewPassword { get; init; }
}
