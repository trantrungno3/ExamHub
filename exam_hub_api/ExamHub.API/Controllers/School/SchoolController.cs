using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using SchoolEntity = ExamHub.Core.Domain.Entities.School;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý trường học</summary>
[ApiController]
[Route("api/[controller]")]
public class SchoolController(ISchoolService service)
    : CategoryBaseController<SchoolEntity, int, SchoolRequest, SchoolResponse>(service)
{
    /// <inheritdoc/>
    protected override SchoolEntity ToEntity(SchoolRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override SchoolEntity ToEntityForUpdate(int id, SchoolRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override SchoolResponse ToResponse(SchoolEntity entity) => SchoolResponse.FromEntity(entity);

    /// <summary>Lấy theo mã trường</summary>
    [HttpGet("code/{code}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<SchoolResponse>>> GetByCode(string code, CancellationToken ct = default)
    {
        var result = await service.GetByCodeAsync(code, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<SchoolResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Lấy trường kèm danh sách khoá học</summary>
    [HttpGet("{id:int}/with-cohorts")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<SchoolResponse>>> GetWithCohorts(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithCohortsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<SchoolResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Lấy trường kèm danh sách thành viên</summary>
    [HttpGet("{id:int}/with-members")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<SchoolResponse>>> GetWithMembers(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithMembersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<SchoolResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }
}
