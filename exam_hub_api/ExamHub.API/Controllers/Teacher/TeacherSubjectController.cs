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
    [HttpGet("teacher/{userId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<TeacherSubject>>>> GetByTeacher(Guid userId, CancellationToken ct)
    {
        var result = await service.GetByTeacherAsync(userId, ct);
        var list = result.ToList();
        return Ok(RequestResponse<IReadOnlyList<TeacherSubject>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Kiểm tra giáo viên có phụ trách môn học không</summary>
    [HttpGet("teacher/{userId:guid}/subject/{subjectId:int}/check")]
    public async Task<ActionResult<RequestResponse<bool>>> IsTeacherOfSubject(Guid userId, int subjectId, CancellationToken ct)
    {
        var result = await service.IsTeacherOfSubjectAsync(userId, subjectId, ct);
        return Ok(RequestResponse<bool>.Success("Kiểm tra thành công!", result, 1));
    }

    /// <summary>Gán môn học cho giáo viên</summary>
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(
        [FromBody] TeacherSubjectAssignRequest request,
        CancellationToken ct)
    {
        await service.AssignSubjectAsync(request.UserId, request.SubjectId, ct);
        return NoContent();
    }

    /// <summary>Xóa phụ trách môn học</summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(
        [FromBody] TeacherSubjectAssignRequest request,
        CancellationToken ct)
    {
        await service.RemoveSubjectAsync(request.UserId, request.SubjectId, ct);
        return NoContent();
    }
}

/// <summary>Request DTO gán / xóa môn học giáo viên</summary>
public record TeacherSubjectAssignRequest(Guid UserId, int SubjectId);
