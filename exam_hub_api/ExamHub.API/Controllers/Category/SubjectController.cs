using ExamHub.Core.DataTransferObjects.Category;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Category;

/// <summary>Controller quản lý môn học</summary>
[ApiController]
[Route("api/[controller]")]
public class SubjectController(ISubjectService service)
    : CategoryBaseController<Subject, int, SubjectRequest, SubjectResponse>(service)
{
    /// <inheritdoc/>
    protected override Subject ToEntity(SubjectRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override Subject ToEntityForUpdate(int id, SubjectRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override SubjectResponse ToResponse(Subject entity) => SubjectResponse.FromEntity(entity);

    /// <summary>Lấy theo khối lớp</summary>
    [HttpGet("by-grade/{gradeLevelId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<SubjectResponse>>> GetByGradeLevel(int gradeLevelId, CancellationToken ct = default)
    {
        var result = await service.GetByGradeLevelAsync(gradeLevelId, ct);
        return Ok(result.Select(ToResponse).ToList());
    }

    /// <summary>Lấy kèm chủ đề</summary>
    [HttpGet("{id:int}/with-topics")]
    [Authorize]
    public async Task<ActionResult<SubjectResponse>> GetWithTopics(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithTopicsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(ToResponse(result));
    }
}
