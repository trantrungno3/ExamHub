using ExamHub.Core.DataTransferObjects.Category;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Category;

/// <summary>Controller quản lý khối lớp</summary>
[ApiController]
[Route("api/[controller]")]
public class GradeLevelController(IGradeLevelService service)
    : CategoryBaseController<GradeLevel, int, GradeLevelRequest, GradeLevelResponse>(service)
{
    /// <inheritdoc/>
    protected override GradeLevel ToEntity(GradeLevelRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override GradeLevel ToEntityForUpdate(int id, GradeLevelRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override GradeLevelResponse ToResponse(GradeLevel entity) => GradeLevelResponse.FromEntity(entity);

    /// <summary>Lấy kèm môn học</summary>
    [HttpGet("{id:int}/with-subjects")]
    [Authorize]
    public async Task<ActionResult<GradeLevelResponse>> GetWithSubjects(int id, CancellationToken ct = default)
    {
        var result = await service.GetWithSubjectsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(ToResponse(result));
    }
}
