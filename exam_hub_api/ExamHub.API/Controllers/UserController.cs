using ExamHub.Core;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers;

/// <summary>Controller quản lý người dùng và phân quyền</summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UserController(
    IUserManagementService userService,
    IUserBulkImportService bulkUserImportService) : AuthorizeControllerBase
{
    // ── Quản lý người dùng ──────────────────────────────────────

    /// <summary>Lấy danh sách toàn bộ người dùng</summary>
    [HttpGet]
    public ActionResult<RequestResponse<IReadOnlyList<UserResponse>>> GetAll()
    {
        var list = userService.GetList().Select(UserResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<UserResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy người dùng theo ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<UserResponse>>> GetById(Guid id)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(RequestResponse<UserResponse>.Success("Lấy dữ liệu thành công!", UserResponse.FromEntity(user), 1));
    }

    /// <summary>Tạo người dùng mới</summary>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<UserResponse>>> Create([FromBody] CreateUserRequest request)
    {
        if (await userService.CheckUserNameExistAsync(request.UserName))
            return Conflict(RequestResponse<UserResponse>.Error("Tên đăng nhập đã tồn tại!"));

        var result = await userService.CreateAsync(request);
        if (result is null)
            return StatusCode(500, RequestResponse<UserResponse>.Error("Tạo người dùng thất bại!"));

        return StatusCode(201,
            RequestResponse<UserResponse>.Success("Tạo người dùng thành công!", UserResponse.FromEntity(result), 1));
    }

    /// <summary>Import người dùng hàng loạt từ file Excel (.xlsx)</summary>
    [HttpPost("bulk-import")]
    public async Task<ActionResult<RequestResponse<BulkUserImportResponse>>> BulkImport(
        [FromForm] BulkUserImportRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(RequestResponse<object>.Error("File import không được để trống."));
        if (!request.File.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(RequestResponse<object>.Error("Chỉ chấp nhận file Excel (.xlsx)."));
        if (string.IsNullOrWhiteSpace(request.DefaultPassword))
            return BadRequest(RequestResponse<object>.Error("Mật khẩu mặc định không được để trống."));

        var result = await bulkUserImportService.ImportAsync(request, ct);
        return Ok(RequestResponse<BulkUserImportResponse>.Success(
            $"Import hoàn tất: {result.SuccessCount} thành công, {result.ErrorCount} lỗi.",
            result, result.SuccessCount));
    }

    /// <summary>Tải file Excel mẫu để import người dùng</summary>
    [HttpGet("bulk-import/template")]
    public IActionResult DownloadImportTemplate()
    {
        var bytes = bulkUserImportService.BuildTemplate();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "user-import-template.xlsx");
    }

    /// <summary>Cập nhật thông tin người dùng</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequestResponse<UserResponse>>> Update(Guid id,
        [FromBody] UpdateUserRequest request)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();

        var updated = await userService.UpdateAsync(user, request);
        return Ok(RequestResponse<UserResponse>.Success("Cập nhật thành công!", UserResponse.FromEntity(updated), 1));
    }

    /// <summary>Xóa người dùng</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();
        await userService.DeleteAsync(user);
        return NoContent();
    }

    /// <summary>Khóa / mở khóa tài khoản</summary>
    [HttpPatch("{id:guid}/lock")]
    public async Task<ActionResult<RequestResponse<bool>>> SetLock(Guid id, [FromBody] bool isLocked)
    {
        if (!await userService.CheckUserExistByIdAsync(id)) return NotFound();
        await userService.SetLockAsync(id, isLocked);
        var msg = isLocked ? "Khóa tài khoản thành công!" : "Mở khóa tài khoản thành công!";
        return Ok(RequestResponse<bool>.Success(msg, isLocked, 1));
    }

    /// <summary>Đặt lại mật khẩu cho người dùng</summary>
    [HttpPatch("{id:guid}/reset-password")]
    public async Task<ActionResult<RequestResponse<bool>>> ResetPassword(Guid id,
        [FromBody] ResetPasswordRequest request)
    {
        if (!await userService.CheckUserExistByIdAsync(id)) return NotFound();
        await userService.ResetPasswordAsync(id, request.NewPassword);
        return Ok(RequestResponse<bool>.Success("Đặt lại mật khẩu thành công!", true, 1));
    }

    // ── Phân quyền (Roles) ──────────────────────────────────────

    /// <summary>Lấy danh sách role của người dùng</summary>
    [HttpGet("{id:guid}/roles")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<string>>>> GetRoles(Guid id)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();
        var roles = user.Roles.ToList();
        return Ok(RequestResponse<IReadOnlyList<string>>.Success("Lấy danh sách role thành công!", roles, roles.Count));
    }

    /// <summary>Đặt lại toàn bộ roles (thay thế tất cả)</summary>
    [HttpPut("{id:guid}/roles")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<string>>>> SetRoles(Guid id,
        [FromBody] SetRolesRequest request)
    {
        if (!await userService.CheckUserExistByIdAsync(id)) return NotFound();
        await userService.SetRolesAsync(id, request.Roles);
        var roles = request.Roles.ToList();
        return Ok(RequestResponse<IReadOnlyList<string>>.Success("Cập nhật roles thành công!", roles, roles.Count));
    }

    /// <summary>Thêm một role cho người dùng</summary>
    [HttpPost("{id:guid}/roles/{role}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<string>>>> AddRole(Guid id, string role)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await userService.AddRoleAsync(user, role);
        if (roles is null)
            return Conflict(RequestResponse<IReadOnlyList<string>>.Error($"Người dùng đã có role '{role}'!"));

        return Ok(RequestResponse<IReadOnlyList<string>>.Success("Thêm role thành công!", roles.ToList(), roles.Length));
    }

    /// <summary>Xóa một role khỏi người dùng</summary>
    [HttpDelete("{id:guid}/roles/{role}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<string>>>> RemoveRole(Guid id, string role)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await userService.RemoveRoleAsync(user, role);
        if (roles is null)
            return NotFound(RequestResponse<IReadOnlyList<string>>.Error($"Người dùng không có role '{role}'!"));

        return Ok(RequestResponse<IReadOnlyList<string>>.Success("Xóa role thành công!", roles.ToList(), roles.Length));
    }
}
