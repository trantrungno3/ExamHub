using ExamHub.Core;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.User;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVT.Core;

namespace ExamHub.API.Controllers;

/// <summary>Controller quản lý người dùng và phân quyền</summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UserController(
    IUserManagementService userService,
    IUserBulkImportService bulkUserImportService,
    IExamSubmissionRepository submissionRepo) : AuthorizeControllerBase
{
    // ── Quản lý người dùng ──────────────────────────────────────

    /// <summary>Lấy danh sách toàn bộ người dùng</summary>
    /// <returns>Danh sách toàn bộ người dùng.</returns>
    [HttpGet]
    public ActionResult<RequestResponse<IReadOnlyList<UserResponse>>> GetAll()
    {
        var list = userService.GetList().Select(UserResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<UserResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy người dùng theo ID</summary>
    /// <param name="id">Id người dùng cần lấy.</param>
    /// <returns>Bản ghi tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<UserResponse>>> GetById(Guid id)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(RequestResponse<UserResponse>.Success("Lấy dữ liệu thành công!", UserResponse.FromEntity(user), 1));
    }

    /// <summary>Tạo người dùng mới</summary>
    /// <param name="request">Thông tin tài khoản cần tạo.</param>
    /// <returns>Người dùng vừa tạo (HTTP 201); 409 nếu tên đăng nhập đã tồn tại.</returns>
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
    /// <param name="request">File Excel + mật khẩu mặc định cho tài khoản mới.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả import kèm số dòng thành công/lỗi.</returns>
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
    /// <returns>File .xlsx mẫu với đúng thứ tự cột mà <c>BulkImport</c> yêu cầu.</returns>
    [HttpGet("bulk-import/template")]
    public IActionResult DownloadImportTemplate()
    {
        var bytes = bulkUserImportService.BuildTemplate();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "user-import-template.xlsx");
    }

    /// <summary>Cập nhật thông tin người dùng</summary>
    /// <param name="id">Id người dùng cần cập nhật.</param>
    /// <param name="request">Thông tin mới.</param>
    /// <returns>Người dùng sau khi cập nhật; 404 nếu không tồn tại.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequestResponse<UserResponse>>> Update(Guid id,
        [FromBody] UpdateUserRequest request)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();

        var updated = await userService.UpdateAsync(user, request);
        return Ok(RequestResponse<UserResponse>.Success("Cập nhật thành công!", UserResponse.FromEntity(updated), 1));
    }

    /// <summary>Xóa người dùng. Nếu còn bài nộp/đề thi liên quan, trả về 409 trừ khi <paramref name="force"/> = true.</summary>
    /// <param name="id">Id người dùng cần xoá.</param>
    /// <param name="force">True để xoá cưỡng bức kèm mọi bài nộp liên quan.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công; 404 nếu không tồn tại; 409 nếu còn dữ liệu liên quan và <paramref name="force"/> = false.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] bool force = false, CancellationToken ct = default)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();

        var submissions = await submissionRepo.GetByStudentAsync(id, ct);
        if (submissions.Count > 0 && !force)
            return Conflict(RequestResponse<object>.Error(
                "Người dùng đang có dữ liệu liên quan (bài làm/đề thi). Dùng xoá bắt buộc nếu chắc chắn."));

        // FK exam_submissions.student_id không có ON DELETE CASCADE → dọn bài nộp trước khi xoá user.
        // Ngoài ra còn các FK khác tới app_users (vd. submission_answers.graded_by,
        // questions.verified_by) không có repository/kiểm tra riêng ở đây — nếu người dùng còn
        // được tham chiếu qua các bảng đó, DeleteAsync bên dưới sẽ ném DbUpdateException do vi
        // phạm khoá ngoại; bắt chung để trả về 409 tiếng Việt thay vì để lộ lỗi 500 (không có
        // middleware xử lý exception toàn cục trong codebase này).
        try
        {
            if (force)
                foreach (var submission in submissions)
                    await submissionRepo.DeleteAsync(submission, ct);

            await userService.DeleteAsync(user);
        }
        catch (DbUpdateException)
        {
            return Conflict(RequestResponse<object>.Error(
                "Không thể xoá người dùng do còn dữ liệu liên quan (bài chấm, câu hỏi đã duyệt, ...)."));
        }

        return NoContent();
    }

    /// <summary>Khóa / mở khóa tài khoản</summary>
    /// <param name="id">Id người dùng cần đổi trạng thái khoá.</param>
    /// <param name="isLocked">True để khoá tài khoản, false để mở khoá.</param>
    /// <returns>Trạng thái khoá sau khi cập nhật; 404 nếu không tồn tại.</returns>
    [HttpPatch("{id:guid}/lock")]
    public async Task<ActionResult<RequestResponse<bool>>> SetLock(Guid id, [FromBody] bool isLocked)
    {
        if (!await userService.CheckUserExistByIdAsync(id)) return NotFound();
        await userService.SetLockAsync(id, isLocked);
        var msg = isLocked ? "Khóa tài khoản thành công!" : "Mở khóa tài khoản thành công!";
        return Ok(RequestResponse<bool>.Success(msg, isLocked, 1));
    }

    /// <summary>Đặt lại mật khẩu cho người dùng</summary>
    /// <param name="id">Id người dùng cần đặt lại mật khẩu.</param>
    /// <param name="request">Mật khẩu mới.</param>
    /// <returns>Kết quả đặt lại mật khẩu; 404 nếu không tồn tại.</returns>
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
    /// <param name="id">Id người dùng cần tra cứu.</param>
    /// <returns>Danh sách role; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}/roles")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<string>>>> GetRoles(Guid id)
    {
        var user = await userService.FindByIdAsync(id);
        if (user is null) return NotFound();
        var roles = user.Roles.ToList();
        return Ok(RequestResponse<IReadOnlyList<string>>.Success("Lấy danh sách role thành công!", roles, roles.Count));
    }

    /// <summary>Đặt lại toàn bộ roles (thay thế tất cả)</summary>
    /// <param name="id">Id người dùng cần cập nhật.</param>
    /// <param name="request">Danh sách roles mới (thay thế toàn bộ roles cũ).</param>
    /// <returns>Danh sách roles sau khi cập nhật; 404 nếu không tồn tại.</returns>
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
    /// <param name="id">Id người dùng cần thêm role.</param>
    /// <param name="role">Tên role cần thêm.</param>
    /// <returns>Danh sách roles sau khi thêm; 404 nếu người dùng không tồn tại; 409 nếu đã có role này.</returns>
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
    /// <param name="id">Id người dùng cần xoá role.</param>
    /// <param name="role">Tên role cần xoá.</param>
    /// <returns>Danh sách roles sau khi xoá; 404 nếu người dùng không tồn tại hoặc không có role này.</returns>
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
