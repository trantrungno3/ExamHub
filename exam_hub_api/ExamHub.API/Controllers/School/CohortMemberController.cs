using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý học sinh trong khoá học</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CohortMemberController(ICohortMemberService service) : ControllerBase
{
    /// <summary>Lấy theo ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CohortMemberResponse>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(CohortMemberResponse.FromEntity(result));
    }

    /// <summary>Lấy danh sách học sinh theo khoá</summary>
    [HttpGet("by-cohort/{cohortId:int}")]
    public async Task<ActionResult<IReadOnlyList<CohortMemberResponse>>> GetByCohort(int cohortId, CancellationToken ct = default)
    {
        var result = await service.GetByCohortAsync(cohortId, ct);
        return Ok(result.Select(CohortMemberResponse.FromEntity).ToList());
    }

    /// <summary>Lấy các khoá học của một học sinh</summary>
    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CohortMemberResponse>>> GetByStudent(Guid studentId, CancellationToken ct = default)
    {
        var result = await service.GetByStudentAsync(studentId, ct);
        return Ok(result.Select(CohortMemberResponse.FromEntity).ToList());
    }

    /// <summary>Thêm học sinh vào khoá học</summary>
    [HttpPost("")]
    public async Task<ActionResult<CohortMemberResponse>> AddStudent([FromBody] CohortMemberRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity();
        var result = await service.AddStudentAsync(entity, ct);
        return Ok(CohortMemberResponse.FromEntity(result));
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
    public async Task<ActionResult<bool>> SetActive(Guid id, [FromBody] bool isActive, CancellationToken ct = default)
    {
        var result = await service.SetActiveAsync(id, isActive, ct);
        return Ok(result);
    }
}
