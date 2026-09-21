using System.Diagnostics;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.Teacher;

/// <summary>Controller quản lý phân công môn học cho giáo viên</summary>
[ApiController]
[Route("api/teacher-subjects")]
public class TeacherSubjectController(ITeacherSubjectService service) : AuthorizeControllerBase
{
    /// <summary>Lấy danh sách môn học của giáo viên</summary>
    /// <param name="userId">Id giáo viên cần tra cứu.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách phân công môn học của giáo viên.</returns>
    [HttpGet("teacher/{userId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<TeacherSubject>>>> GetByTeacher(Guid userId, CancellationToken ct)
    {
        var result = await service.GetByTeacherAsync(userId, ct);
        var list = result.ToList();
        return Ok(RequestResponse<IReadOnlyList<TeacherSubject>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Kiểm tra giáo viên có phụ trách môn học không</summary>
    /// <param name="userId">Id giáo viên cần kiểm tra.</param>
    /// <param name="subjectId">Id môn học cần kiểm tra.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>True nếu giáo viên phụ trách môn học này.</returns>
    [HttpGet("teacher/{userId:guid}/subject/{subjectId:int}/check")]
    public async Task<ActionResult<RequestResponse<bool>>> IsTeacherOfSubject(Guid userId, int subjectId, CancellationToken ct)
    {
        var result = await service.IsTeacherOfSubjectAsync(userId, subjectId, ct);
        return Ok(RequestResponse<bool>.Success("Kiểm tra thành công!", result, 1));
    }

    /// <summary>Gán môn học cho giáo viên</summary>
    /// <param name="request">Id giáo viên và id môn học cần gán.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi gán thành công.</returns>
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(
        [FromBody] TeacherSubjectAssignRequest request,
        CancellationToken ct)
    {
        await service.AssignSubjectAsync(request.UserId, request.SubjectId, ct);
        return NoContent();
    }

    /// <summary>Xóa phụ trách môn học</summary>
    /// <param name="request">Id giáo viên và id môn học cần gỡ.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công.</returns>
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(
        [FromBody] TeacherSubjectAssignRequest request,
        CancellationToken ct)
    {
        await service.RemoveSubjectAsync(request.UserId, request.SubjectId, ct);
        return NoContent();
    }

    /// <summary>Đặt lại toàn bộ danh sách môn học phụ trách của giáo viên (gán/gỡ nhiều môn 1 lần)</summary>
    /// <param name="userId">Id giáo viên cần cập nhật.</param>
    /// <param name="request">Danh sách id môn học cuối cùng giáo viên phụ trách.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi cập nhật thành công.</returns>
    [HttpPut("teacher/{userId:guid}/subjects")]
    public async Task<IActionResult> SetSubjects(
        Guid userId,
        [FromBody] SetTeacherSubjectsRequest request,
        CancellationToken ct)
    {
        await service.SetSubjectsAsync(userId, request.SubjectIds, ct);
        return NoContent();
    }
}

/// <summary>Request DTO gán / xóa môn học giáo viên</summary>
public record TeacherSubjectAssignRequest(Guid UserId, int SubjectId);

/// <summary>Request DTO đặt lại toàn bộ danh sách môn học phụ trách của giáo viên</summary>
public record SetTeacherSubjectsRequest(int[] SubjectIds);
