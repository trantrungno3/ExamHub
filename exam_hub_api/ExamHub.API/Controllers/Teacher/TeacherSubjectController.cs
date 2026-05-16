using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Teacher;

/// <summary>Controller quản lý phân công môn học cho giáo viên</summary>
[ApiController]
[Authorize]
[Route("api/teacher-subjects")]
public class TeacherSubjectController(ITeacherSubjectService service) : ControllerBase
{
    /// <summary>Lấy danh sách môn học của giáo viên</summary>
    [HttpGet("teacher/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TeacherSubject>>> GetByTeacher(Guid userId, CancellationToken ct)
    {
        var result = await service.GetByTeacherAsync(userId, ct);
        return Ok(result);
    }

    /// <summary>Kiểm tra giáo viên có phụ trách môn học không</summary>
    [HttpGet("teacher/{userId:guid}/subject/{subjectId:int}/check")]
    public async Task<ActionResult<bool>> IsTeacherOfSubject(Guid userId, int subjectId, CancellationToken ct)
    {
        var result = await service.IsTeacherOfSubjectAsync(userId, subjectId, ct);
        return Ok(result);
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
