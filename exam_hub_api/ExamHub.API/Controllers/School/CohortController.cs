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
    /// <param name="schoolId">Id trường cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách khoá học thuộc trường.</returns>
    [HttpGet("by-school/{schoolId:int}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortResponse>>>> GetBySchool(int schoolId, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolAsync(schoolId, ct);
        var list = result.Select(ToResponse).ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy khoá học kèm danh sách lớp học</summary>
    /// <param name="id">Id khoá học cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Khoá học kèm danh sách lớp; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:int}/with-classes")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<CohortResponse>>> GetWithClasses(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithClassesAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CohortResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Lấy khoá học kèm danh sách học sinh</summary>
    /// <param name="id">Id khoá học cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Khoá học kèm danh sách học sinh; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:int}/with-members")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<CohortResponse>>> GetWithMembers(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithMembersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CohortResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>
    /// Xoá bắt buộc khoá học kèm toàn bộ dữ liệu liên quan.
    /// Chỉ Admin: đây là thao tác cascade không hoàn tác được (lớp, thành viên, phân công kỳ thi).
    /// </summary>
    /// <param name="id">Id khoá học cần xoá cưỡng bức.</param>
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
