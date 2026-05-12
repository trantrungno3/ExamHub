using System.ComponentModel.DataAnnotations;
using TVT.Core.Extensions;
using TVT.Core.IdentityUser.PostgreSql.Models;
using TVT.Core.Utils;

namespace ExamHub.Core.DataAccessObjects;

/// <summary>
/// </summary>
public record AccountDto
{
    /// <summary>
    /// </summary>
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string UserName { get; set; }

    /// <summary>
    /// </summary>
    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string Password { get; set; }
}

/// <summary>
/// </summary>
public sealed record LoginDto : AccountDto
{
    /// <summary>
    /// </summary>
    public bool IsRemember { get; set; }
}

/// <summary>
/// </summary>
public record RegisterDto : AccountDto
{
    /// <summary>
    /// </summary>
    [Display(Name = "Tên hiển thị")]
    [Required(ErrorMessage = DataAnnotationErrorText.Required)]
    public required string DisplayName { get; set; }

    /// <summary>
    /// </summary>
    public string? PhoneNumber { get; set; }

    public UserAdmin ToDomain()
    {
        return new UserAdmin
        {
            UserName = UserName,
            DisplayName = DisplayName,
            PhoneNumber = PhoneNumber,
            PasswordHash = Password,
            NormalizedUserName = UserName.ToUpperNormalize()
        };
    }
}