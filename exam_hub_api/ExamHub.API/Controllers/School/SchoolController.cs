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
    /// <param name="code">Mã trường cần tra cứu.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("code/{code}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<SchoolResponse>>> GetByCode(string code, CancellationToken ct = default)
    {
        var result = await service.GetByCodeAsync(code, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<SchoolResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Lấy trường kèm danh sách khoá học</summary>
    /// <param name="id">Id trường cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Trường kèm danh sách khoá học; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:int}/with-cohorts")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<SchoolResponse>>> GetWithCohorts(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithCohortsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<SchoolResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Lấy trường kèm danh sách thành viên</summary>
    /// <param name="id">Id trường cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Trường kèm danh sách thành viên; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:int}/with-members")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<SchoolResponse>>> GetWithMembers(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithMembersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<SchoolResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>
    /// Xoá bắt buộc trường học kèm toàn bộ dữ liệu liên quan.
    /// Chỉ Admin: đây là thao tác cascade không hoàn tác được (khoá học, lớp, thành viên).
    /// </summary>
    /// <param name="id">Id trường cần xoá cưỡng bức.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công.</returns>
    [HttpDelete("{id:int}/force")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ForceDelete(int id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, true, ct);
        return NoContent();
    }
}
