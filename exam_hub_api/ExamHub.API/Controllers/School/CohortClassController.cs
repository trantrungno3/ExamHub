using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý lớp học trong khoá</summary>
[ApiController]
[Route("api/[controller]")]
public class CohortClassController(ICohortClassService service) : ControllerBase
{
    /// <summary>Lấy theo ID</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<CohortClassResponse>> GetById(int id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(CohortClassResponse.FromEntity(result));
    }

    /// <summary>Lấy danh sách lớp học theo khoá</summary>
    [HttpGet("by-cohort/{cohortId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CohortClassResponse>>> GetByCohort(int cohortId, CancellationToken ct = default)
    {
        var result = await service.GetByCohortAsync(cohortId, ct);
        return Ok(result.Select(CohortClassResponse.FromEntity).ToList());
    }

    /// <summary>Lấy danh sách lớp học theo năm học</summary>
    [HttpGet("by-school-year/{schoolYear}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CohortClassResponse>>> GetBySchoolYear(string schoolYear, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolYearAsync(schoolYear, ct);
        return Ok(result.Select(CohortClassResponse.FromEntity).ToList());
    }

    /// <summary>Cập nhật giáo viên chủ nhiệm</summary>
    [HttpPatch("{id:int}/homeroom-teacher")]
    [Authorize]
    public async Task<ActionResult<bool>> SetHomeroomTeacher(int id, [FromBody] SetHomeroomTeacherRequest request, CancellationToken ct = default)
    {
        var result = await service.SetHomeroomTeacherAsync(id, request.TeacherId, ct);
        return Ok(result);
    }
}
