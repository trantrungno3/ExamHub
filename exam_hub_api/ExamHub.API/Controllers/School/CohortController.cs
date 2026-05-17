using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý khoá học</summary>
[ApiController]
[Route("api/[controller]")]
public class CohortController(ICohortService service)
    : CategoryBaseController<Cohort, int, CohortRequest, CohortResponse>(service)
{
    /// <inheritdoc/>
    protected override Cohort ToEntity(CohortRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override Cohort ToEntityForUpdate(int id, CohortRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override CohortResponse ToResponse(Cohort entity) => CohortResponse.FromEntity(entity);

    /// <summary>Lấy danh sách khoá học theo trường</summary>
    [HttpGet("by-school/{schoolId:int}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortResponse>>>> GetBySchool(int schoolId, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolAsync(schoolId, ct);
        var list = result.Select(ToResponse).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy khoá học kèm danh sách lớp học</summary>
    [HttpGet("{id:int}/with-classes")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<CohortResponse>>> GetWithClasses(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithClassesAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CohortResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Lấy khoá học kèm danh sách học sinh</summary>
    [HttpGet("{id:int}/with-members")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<CohortResponse>>> GetWithMembers(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithMembersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CohortResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }
}
