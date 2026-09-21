using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TVT.Core;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý thành viên trường học</summary>
[ApiController]
[Route("api/[controller]")]
public class SchoolMemberController(
    ISchoolMemberService service,
    ISchoolMemberBulkService bulkService) : AuthorizeControllerBase
{
    private const long MaxImportBytes = 10 * 1024 * 1024;

    /// <summary>Kiểm tra metadata file import; trả về thông báo lỗi hoặc null khi hợp lệ.</summary>
    private static string? ValidateImportFile(IFormFile? file)
    {
        if (file is null || file.Length == 0) return "File import không được để trống.";
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return "Chỉ chấp nhận file Excel (.xlsx).";
        return file.Length > MaxImportBytes ? "File import không được vượt quá 10 MB." : null;
    }

    /// <summary>Lấy theo ID</summary>
    /// <param name="id">Id thành viên cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<SchoolMemberResponse>>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<SchoolMemberResponse>.Success("Lấy dữ liệu thành công!", SchoolMemberResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy danh sách thành viên theo trường</summary>
    /// <param name="schoolId">Id trường cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách thành viên thuộc trường.</returns>
    [HttpGet("by-school/{schoolId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<SchoolMemberResponse>>>> GetBySchool(int schoolId, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolAsync(schoolId, ct);
        var list = result.Select(SchoolMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<SchoolMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy danh sách thành viên theo trường và vai trò</summary>
    /// <param name="schoolId">Id trường cần lọc.</param>
    /// <param name="role">Vai trò cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách thành viên thuộc trường và có vai trò tương ứng.</returns>
    [HttpGet("by-school/{schoolId:int}/role/{role}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<SchoolMemberResponse>>>> GetBySchoolAndRole(int schoolId, string role, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolAndRoleAsync(schoolId, role, ct);
        var list = result.Select(SchoolMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<SchoolMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy tất cả trường mà một người dùng thuộc vào</summary>
    /// <param name="userId">Id người dùng cần tra cứu.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách bản ghi thành viên của người dùng ở các trường.</returns>
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<SchoolMemberResponse>>>> GetByUser(Guid userId, CancellationToken ct = default)
    {
        var result = await service.GetByUserAsync(userId, ct);
        var list = result.Select(SchoolMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<SchoolMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Thêm thành viên vào trường</summary>
    /// <param name="request">Thông tin thành viên cần thêm.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Thành viên vừa thêm.</returns>
    [HttpPost("")]
    public async Task<ActionResult<RequestResponse<SchoolMemberResponse>>> AddMember([FromBody] SchoolMemberRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity();
        var result = await service.AddMemberAsync(entity, ct);
        return Ok(RequestResponse<SchoolMemberResponse>.Success("Thêm thành viên thành công!", SchoolMemberResponse.FromEntity(result), 1));
    }

    /// <summary>Cập nhật vai trò thành viên</summary>
    /// <param name="id">Id thành viên cần cập nhật.</param>
    /// <param name="request">Dữ liệu cập nhật.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Thành viên sau khi cập nhật.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequestResponse<SchoolMemberResponse>>> Update(Guid id, [FromBody] SchoolMemberRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity();
        entity.Id = id;
        var result = await service.UpdateAsync(entity, ct);
        return Ok(RequestResponse<SchoolMemberResponse>.Success("Cập nhật thành công!", SchoolMemberResponse.FromEntity(result), 1));
    }

    /// <summary>Xóa thành viên khỏi trường</summary>
    /// <param name="id">Id thành viên cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, CancellationToken ct = default)
    {
        await service.RemoveMemberAsync(id, ct);
        return NoContent();
    }

    /// <summary>Bật/tắt trạng thái thành viên</summary>
    /// <param name="id">Id thành viên cần đổi trạng thái.</param>
    /// <param name="isActive">Trạng thái kích hoạt mới.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Trạng thái kích hoạt sau khi cập nhật.</returns>
    [HttpPatch("{id:guid}/active")]
    public async Task<ActionResult<RequestResponse<bool>>> SetActive(Guid id, [FromBody] bool isActive, CancellationToken ct = default)
    {
        var result = await service.SetActiveAsync(id, isActive, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật trạng thái thành công!", result, 1));
    }

    /// <summary>Thêm nhiều tài khoản có sẵn vào trường trong một lần</summary>
    /// <param name="request">Vai trò, danh sách người dùng và vị trí khoá/lớp khi là Student.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Số dòng thành công và danh sách lỗi theo vị trí lựa chọn.</returns>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RequestResponse<SchoolMemberBulkResult>>> BulkAdd(
        [FromBody] SchoolMemberBulkAddRequest request, CancellationToken ct)
    {
        var result = await bulkService.AddAsync(request, ct);
        return Ok(RequestResponse<SchoolMemberBulkResult>.Success(
            $"Thêm hoàn tất: {result.SuccessCount} thành công, {result.ErrorCount} lỗi.",
            result, result.SuccessCount));
    }

    /// <summary>Kiểm tra file Excel thành viên mà không ghi dữ liệu</summary>
    /// <param name="request">Trường đích và file .xlsx cần kiểm tra.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Từng dòng kèm trạng thái hợp lệ và lỗi; 400 nếu file sai định dạng.</returns>
    [HttpPost("bulk-import/preview")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write-heavy")]
    public async Task<ActionResult<RequestResponse<SchoolMemberImportPreviewResponse>>> Preview(
        [FromForm] SchoolMemberImportRequest request, CancellationToken ct)
    {
        var error = ValidateImportFile(request.File);
        if (error is not null) return BadRequest(RequestResponse<object>.Error(error));
        try
        {
            var result = await bulkService.PreviewAsync(request, ct);
            return Ok(RequestResponse<SchoolMemberImportPreviewResponse>.Success(
                "Kiểm tra file thành công.", result, result.Rows.Count));
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(RequestResponse<object>.Error(ex.Message));
        }
    }

    /// <summary>Import các dòng hợp lệ từ file Excel thành viên</summary>
    /// <param name="request">Trường đích và file .xlsx cần import.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Số dòng ghi thành công và lỗi theo số dòng trong file; 400 nếu file sai định dạng.</returns>
    [HttpPost("bulk-import")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write-heavy")]
    public async Task<ActionResult<RequestResponse<SchoolMemberBulkResult>>> BulkImport(
        [FromForm] SchoolMemberImportRequest request, CancellationToken ct)
    {
        var error = ValidateImportFile(request.File);
        if (error is not null) return BadRequest(RequestResponse<object>.Error(error));
        try
        {
            var result = await bulkService.ImportAsync(request, ct);
            return Ok(RequestResponse<SchoolMemberBulkResult>.Success(
                $"Import hoàn tất: {result.SuccessCount} thành công, {result.ErrorCount} lỗi.",
                result, result.SuccessCount));
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(RequestResponse<object>.Error(ex.Message));
        }
    }

    /// <summary>Tải file Excel mẫu để import thành viên trường</summary>
    /// <returns>File .xlsx mẫu với đúng thứ tự cột mà <c>BulkImport</c> yêu cầu.</returns>
    [HttpGet("bulk-import/template")]
    [Authorize(Roles = "Admin")]
    public IActionResult DownloadImportTemplate() => File(
        bulkService.BuildTemplate(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "school-member-import-template.xlsx");
}
