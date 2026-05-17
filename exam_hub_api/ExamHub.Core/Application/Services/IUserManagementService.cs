using ExamHub.Core.DataTransferObjects.User;
using TVT.Core.IdentityUser.PostgreSql.Models;

namespace ExamHub.Core.Application.Services;

public interface IUserManagementService
{
    // ── Queries ────────────────────────────────────────────────────
    IEnumerable<UserAdmin> GetList();
    Task<UserAdmin?> FindByIdAsync(Guid id);
    Task<bool> CheckUserNameExistAsync(string userName);
    Task<bool> CheckUserExistByIdAsync(Guid id);

    // ── Commands ───────────────────────────────────────────────────
    /// <summary>Tạo user mới — trả về entity đã lưu, hoặc <c>null</c> nếu DB lỗi.</summary>
    Task<UserAdmin?> CreateAsync(CreateUserRequest request);

    /// <summary>Cập nhật thông tin user, tự stamp ModifyBy/Modified.</summary>
    Task<UserAdmin> UpdateAsync(UserAdmin user, UpdateUserRequest request);

    Task DeleteAsync(UserAdmin user);

    Task SetLockAsync(Guid id, bool isLocked);
    Task ResetPasswordAsync(Guid id, string newPassword);

    // ── Roles ──────────────────────────────────────────────────────
    Task SetRolesAsync(Guid id, string[] roles);

    /// <returns>Roles sau khi thêm, hoặc <c>null</c> nếu user đã có role đó.</returns>
    Task<string[]?> AddRoleAsync(UserAdmin user, string role);

    /// <returns>Roles sau khi xóa, hoặc <c>null</c> nếu user không có role đó.</returns>
    Task<string[]?> RemoveRoleAsync(UserAdmin user, string role);
}
