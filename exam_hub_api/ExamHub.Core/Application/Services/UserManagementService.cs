using System.Linq.Expressions;
using ExamHub.Core.DataTransferObjects.User;
using Microsoft.AspNetCore.Http;
using TVT.Core.Claims;
using TVT.Core.Db.PostgreSql.Services;
using TVT.Core.Db.PostgreSql.SqlBuilder;
using TVT.Core.Extensions;
using TVT.Core.IdentityUser.PostgreSql.Models;

namespace ExamHub.Core.Application.Services;

public sealed class UserManagementService(
    IUserService inner,
    IHttpContextAccessor httpContextAccessor) : IUserManagementService
{
    private string? CurrentUser =>
        httpContextAccessor.HttpContext?.User.FindFirst(ConstClaim.UserName)?.Value;

    // ── Queries ────────────────────────────────────────────────────

    public IEnumerable<UserAdmin> GetList() => inner.GetList();

    public Task<UserAdmin?> FindByIdAsync(Guid id) => inner.FindByIdAsync(id);

    public Task<bool> CheckUserNameExistAsync(string userName) =>
        inner.CheckUserNameExistAsync(userName);

    public Task<bool> CheckUserExistByIdAsync(Guid id) =>
        inner.CheckUserExistByIdAsync(id);

    // ── Commands ───────────────────────────────────────────────────

    public async Task<UserAdmin?> CreateAsync(CreateUserRequest request)
    {
        var entity = request.ToEntity();
        entity.PasswordHash = request.Password.GetPasswordHash(AppCommon.SaltPassHash!);
        if (!string.IsNullOrEmpty(request.Email))
            entity.SetEmail(request.Email);
        return await inner.CreateAsync(entity);
    }

    public async Task<UserAdmin> UpdateAsync(UserAdmin user, UpdateUserRequest request)
    {
        user.DisplayName = request.DisplayName;
        user.PhoneNumber = request.PhoneNumber;
        user.Sex         = request.Sex;
        user.Avartar     = request.Avartar;
        user.Address     = request.Address;
        user.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Email))
            user.SetEmail(request.Email);
        user.ModifiedBy = CurrentUser;
        user.Modified = DateTime.UtcNow;
        await inner.UpdateAsync(user);
        return user;
    }

    public Task DeleteAsync(UserAdmin user) => inner.DeleteAsync(user);

    public Task SetLockAsync(Guid id, bool isLocked) =>
        UpdateWithModifyAsync(id, u => u.LockoutEnabled, isLocked);

    public Task ResetPasswordAsync(Guid id, string newPassword)
    {
        var hash = newPassword.GetPasswordHash(AppCommon.SaltPassHash!);
        return UpdateWithModifyAsync(id, u => u.PasswordHash, hash);
    }

    // ── Roles ──────────────────────────────────────────────────────

    public Task SetRolesAsync(Guid id, string[] roles) =>
        UpdateWithModifyAsync(id, u => u.Roles, roles);

    public async Task<string[]?> AddRoleAsync(UserAdmin user, string role)
    {
        if (user.IsInRole(role)) return null;
        user.AddRole(role);
        await UpdateWithModifyAsync(user.Id, u => u.Roles, user.Roles);
        return user.Roles;
    }

    public async Task<string[]?> RemoveRoleAsync(UserAdmin user, string role)
    {
        if (!user.IsInRole(role)) return null;
        user.RemoveRole(role);
        await UpdateWithModifyAsync(user.Id, u => u.Roles, user.Roles);
        return user.Roles;
    }

    // ── Private helpers ────────────────────────────────────────────

    private Task UpdateWithModifyAsync<TField>(
        Guid id, Expression<Func<UserAdmin, TField>> field, TField value)
        => inner.UpdateFieldsAsync(id,
            FieldUpdate<UserAdmin>.Set(field, value),
            FieldUpdate<UserAdmin>.Set(u => u.ModifiedBy, CurrentUser),
            FieldUpdate<UserAdmin>.Set(u => u.Modified, DateTime.UtcNow));
}
