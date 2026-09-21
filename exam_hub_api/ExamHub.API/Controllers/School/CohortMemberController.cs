using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý học sinh trong khoá học</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CohortMemberController(ICohortMemberService service) : AuthorizeControllerBase
{
    /// <summary>Lấy theo ID</summary>
    /// <param name="id">Id bản ghi thành viên khoá cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<CohortMemberResponse>>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CohortMemberResponse>.Success("Lấy dữ liệu thành công!", CohortMemberResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy danh sách học sinh theo khoá</summary>
    /// <param name="cohortId">Id khoá học cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách học sinh thuộc khoá.</returns>
    [HttpGet("by-cohort/{cohortId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortMemberResponse>>>> GetByCohort(int cohortId, CancellationToken ct = default)
    {
        var result = await service.GetByCohortAsync(cohortId, ct);
        var list = result.Select(CohortMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy danh sách học sinh theo trường (gộp tất cả các khoá của trường)</summary>
    /// <param name="schoolId">Id trường cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách học sinh thuộc trường, gộp mọi khoá học.</returns>
    [HttpGet("by-school/{schoolId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortMemberResponse>>>> GetBySchool(int schoolId, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolAsync(schoolId, ct);
        var list = result.Select(CohortMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy các khoá học của một học sinh</summary>
    /// <param name="studentId">Id học sinh cần tra cứu.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách bản ghi thành viên khoá của học sinh.</returns>
    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortMemberResponse>>>> GetByStudent(Guid studentId, CancellationToken ct = default)
    {
        var result = await service.GetByStudentAsync(studentId, ct);
        var list = result.Select(CohortMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Thêm học sinh vào khoá học</summary>
    /// <param name="request">Thông tin học sinh/khoá học cần thêm.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi thành viên vừa thêm; 409 nếu học sinh đã thuộc khoá học đó.</returns>
    [HttpPost("")]
    public async Task<ActionResult<RequestResponse<CohortMemberResponse>>> AddStudent([FromBody] CohortMemberRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity();
        var result = await service.AddStudentAsync(entity, ct);
        if (result.Status == TVT.Core.Enums.RequestResponseStatus.Error)
            return Ok(RequestResponse<CohortMemberResponse>.Error(result.Message!));
        return Ok(RequestResponse<CohortMemberResponse>.Success(result.Message!, CohortMemberResponse.FromEntity(result.Data!), 1));
    }

    /// <summary>Xóa học sinh khỏi khoá học</summary>
    /// <param name="id">Id bản ghi thành viên khoá cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid id, CancellationToken ct = default)
    {
        await service.RemoveStudentAsync(id, ct);
        return NoContent();
    }

    /// <summary>Bật/tắt trạng thái học sinh trong khoá</summary>
    /// <param name="id">Id bản ghi thành viên khoá cần đổi trạng thái.</param>
    /// <param name="isActive">Trạng thái kích hoạt mới.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Trạng thái kích hoạt sau khi cập nhật.</returns>
    [HttpPatch("{id:guid}/active")]
    public async Task<ActionResult<RequestResponse<bool>>> SetActive(Guid id, [FromBody] bool isActive, CancellationToken ct = default)
    {
        var result = await service.SetActiveAsync(id, isActive, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật trạng thái thành công!", result, 1));
    }

    /// <summary>Đổi lớp (section) của học sinh trong khoá</summary>
    /// <param name="id">Id bản ghi thành viên khoá cần đổi lớp.</param>
    /// <param name="section">Tên lớp (section) mới; null/rỗng để bỏ gán lớp.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả cập nhật.</returns>
    [HttpPatch("{id:guid}/section")]
    public async Task<ActionResult<RequestResponse<bool>>> SetSection(Guid id, [FromBody] string? section, CancellationToken ct = default)
    {
        return Ok(await service.SetSectionAsync(id, section, ct));
    }
}
