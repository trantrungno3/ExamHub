using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller phân công GV giảng dạy cho lớp</summary>
[ApiController]
[Route("api/cohort-class-teachers")]
public class CohortClassTeacherController(ICohortClassTeacherService service) : ControllerBase
{
    /// <summary>Danh sách phân công của một lớp</summary>
    /// <param name="cohortClassId">Id lớp học cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách phân công GV-môn của lớp.</returns>
    [HttpGet("by-class/{cohortClassId:int}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<CohortClassTeacherResponse>>>> GetByClass(int cohortClassId, CancellationToken ct = default)
    {
        var result = await service.GetByClassAsync(cohortClassId, ct);
        var list = result
            .Select(e => new CohortClassTeacherResponse(e.Id, e.CohortClassId, e.SubjectId, e.TeacherId))
            .ToList();
        return Ok(RequestResponse<IReadOnlyList<CohortClassTeacherResponse>>.Success("Lấy danh sách phân công thành công!", list, list.Count));
    }

    /// <summary>Danh sách Id GV hợp lệ để phân công môn cho lớp</summary>
    /// <param name="cohortClassId">Id lớp học cần phân công.</param>
    /// <param name="subjectId">Id môn học cần phân công.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách Id giáo viên đủ điều kiện (phụ trách môn, chưa được phân công trùng).</returns>
    [HttpGet("eligible-teachers")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<Guid>>>> GetEligibleTeachers(
        [FromQuery] int cohortClassId, [FromQuery] int subjectId, CancellationToken ct = default)
    {
        var ids = await service.GetEligibleTeacherIdsAsync(cohortClassId, subjectId, ct);
        return Ok(RequestResponse<IReadOnlyList<Guid>>.Success("Lấy danh sách giáo viên hợp lệ thành công!", ids, ids.Count));
    }

    /// <summary>Phân công GV dạy môn cho lớp (validate + kiểm ràng buộc)</summary>
    /// <param name="request">Thông tin lớp/môn/giáo viên cần phân công.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi phân công vừa tạo.</returns>
    [HttpPost("assign")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<CohortClassTeacherResponse>>> Assign([FromBody] AssignTeacherRequest request, CancellationToken ct = default)
    {
        var result = await service.AssignAsync(request.CohortClassId, request.SubjectId, request.TeacherId, ct);
        if (result.Status == TVT.Core.Enums.RequestResponseStatus.Error)
            return Ok(RequestResponse<CohortClassTeacherResponse>.Error(result.Message!));
        var e = result.Data!;
        var dto = new CohortClassTeacherResponse(e.Id, e.CohortClassId, e.SubjectId, e.TeacherId);
        return Ok(RequestResponse<CohortClassTeacherResponse>.Success(result.Message!, dto, 1));
    }

    /// <summary>Xoá một phân công theo Id</summary>
    /// <param name="id">Id bản ghi phân công cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả xoá.</returns>
    [HttpDelete("remove/{id:int}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<bool>>> Remove(int id, CancellationToken ct = default)
    {
        await service.RemoveAsync(id, ct);
        return Ok(RequestResponse<bool>.Success("Đã xoá phân công!", true, 1));
    }
}
