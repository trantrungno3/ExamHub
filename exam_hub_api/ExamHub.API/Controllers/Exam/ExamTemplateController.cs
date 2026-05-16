using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý mẫu đề thi</summary>
[ApiController]
[Authorize]
[Route("api/exam-templates")]
public class ExamTemplateController(IExamTemplateService service) : ControllerBase
{
    /// <summary>Lấy mẫu đề thi theo ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExamTemplateResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(ExamTemplateResponse.FromEntity(result));
    }

    /// <summary>Lấy mẫu đề thi kèm phần thi</summary>
    [HttpGet("{id:guid}/with-sections")]
    public async Task<ActionResult<ExamTemplateResponse>> GetWithSections(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithSectionsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(ExamTemplateResponse.FromEntity(result, includeSections: true));
    }

    /// <summary>Lấy danh sách mẫu đề thi theo môn học</summary>
    [HttpGet("by-subject/{subjectId:int}")]
    public async Task<ActionResult<IReadOnlyList<ExamTemplateResponse>>> GetBySubject(int subjectId, CancellationToken ct)
    {
        var result = await service.GetBySubjectAsync(subjectId, ct);
        return Ok(result.Select(t => ExamTemplateResponse.FromEntity(t)).ToList());
    }

    /// <summary>Lấy danh sách mẫu đề thi theo lớp học</summary>
    [HttpGet("by-grade/{gradeLevelId:int}")]
    public async Task<ActionResult<IReadOnlyList<ExamTemplateResponse>>> GetByGradeLevel(int gradeLevelId, CancellationToken ct)
    {
        var result = await service.GetByGradeLevelAsync(gradeLevelId, ct);
        return Ok(result.Select(t => ExamTemplateResponse.FromEntity(t)).ToList());
    }

    /// <summary>Tạo mẫu đề thi kèm phần thi</summary>
    [HttpPost]
    public async Task<ActionResult<ExamTemplateResponse>> Create(
        [FromBody] ExamTemplateRequest request,
        CancellationToken ct)
    {
        var userId   = GetCurrentUserId();
        var entity   = request.ToEntity(userId);
        var sections = request.ToSections();
        var result   = await service.CreateAsync(entity, sections, ct);
        return StatusCode(201, ExamTemplateResponse.FromEntity(result));
    }

    /// <summary>Cập nhật mẫu đề thi (tuỳ chọn kèm phần thi mới)</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExamTemplateResponse>> Update(
        Guid id,
        [FromBody] ExamTemplateRequest request,
        CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var entity     = request.ToEntity(existing.CreatedBy);
        entity.Id      = id;
        var sections   = request.ToSections();
        var result     = await service.UpdateAsync(entity, sections, ct);
        return Ok(ExamTemplateResponse.FromEntity(result));
    }

    /// <summary>Xóa mẫu đề thi</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}
