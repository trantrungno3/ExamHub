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
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<CohortMemberResponse>>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CohortMemberResponse>.Success("Lấy dữ liệu thành công!", CohortMemberResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy danh sách học sinh theo khoá</summary>
    [HttpGet("by-cohort/{cohortId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortMemberResponse>>>> GetByCohort(int cohortId, CancellationToken ct = default)
    {
        var result = await service.GetByCohortAsync(cohortId, ct);
        var list = result.Select(CohortMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy các khoá học của một học sinh</summary>
    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortMemberResponse>>>> GetByStudent(Guid studentId, CancellationToken ct = default)
    {
        var result = await service.GetByStudentAsync(studentId, ct);
        var list = result.Select(CohortMemberResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortMemberResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Thêm học sinh vào khoá học</summary>
    [HttpPost("")]
    public async Task<ActionResult<RequestResponse<CohortMemberResponse>>> AddStudent([FromBody] CohortMemberRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity();
        var result = await service.AddStudentAsync(entity, ct);
        return Ok(RequestResponse<CohortMemberResponse>.Success("Thêm học sinh thành công!", CohortMemberResponse.FromEntity(result), 1));
    }

    /// <summary>Xóa học sinh khỏi khoá học</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid id, CancellationToken ct = default)
    {
        await service.RemoveStudentAsync(id, ct);
        return NoContent();
    }

    /// <summary>Bật/tắt trạng thái học sinh trong khoá</summary>
    [HttpPatch("{id:guid}/active")]
    public async Task<ActionResult<RequestResponse<bool>>> SetActive(Guid id, [FromBody] bool isActive, CancellationToken ct = default)
    {
        var result = await service.SetActiveAsync(id, isActive, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật trạng thái thành công!", result, 1));
    }

    /// <summary>Đổi lớp (section) của học sinh trong khoá</summary>
    [HttpPatch("{id:guid}/section")]
    public async Task<ActionResult<RequestResponse<bool>>> SetSection(Guid id, [FromBody] string? section, CancellationToken ct = default)
    {
        var result = await service.SetSectionAsync(id, section, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật lớp thành công!", result, 1));
    }
}
