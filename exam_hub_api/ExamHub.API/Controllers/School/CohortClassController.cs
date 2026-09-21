using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý lớp học trong khoá</summary>
[ApiController]
[Route("api/[controller]")]
public class CohortClassController(ICohortClassService service) : ControllerBase
{
    /// <summary>Lấy theo ID</summary>
    /// <param name="id">Id lớp học cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<CohortClassResponse>>> GetById(int id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CohortClassResponse>.Success("Lấy dữ liệu thành công!", CohortClassResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy danh sách lớp học theo khoá</summary>
    /// <param name="cohortId">Id khoá học cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách lớp học thuộc khoá.</returns>
    [HttpGet("by-cohort/{cohortId:int}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortClassResponse>>>> GetByCohort(int cohortId, CancellationToken ct = default)
    {
        var result = await service.GetByCohortAsync(cohortId, ct);
        var list = result.Select(CohortClassResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortClassResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy danh sách lớp học theo năm học</summary>
    /// <param name="schoolYear">Năm học cần lọc (vd. "2025-2026").</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách lớp học thuộc năm học.</returns>
    [HttpGet("by-school-year/{schoolYear}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortClassResponse>>>> GetBySchoolYear(string schoolYear, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolYearAsync(schoolYear, ct);
        var list = result.Select(CohortClassResponse.FromEntity).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortClassResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Cập nhật giáo viên chủ nhiệm</summary>
    /// <param name="id">Id lớp học cần cập nhật.</param>
    /// <param name="request">Id giáo viên chủ nhiệm mới.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả cập nhật.</returns>
    [HttpPatch("{id:int}/homeroom-teacher")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<bool>>> SetHomeroomTeacher(int id, [FromBody] SetHomeroomTeacherRequest request, CancellationToken ct = default)
    {
        var result = await service.SetHomeroomTeacherAsync(id, request.TeacherId, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật giáo viên chủ nhiệm thành công!", result, 1));
    }
}
