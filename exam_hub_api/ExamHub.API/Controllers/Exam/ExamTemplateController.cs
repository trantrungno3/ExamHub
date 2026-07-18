using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý mẫu đề thi</summary>
[ApiController]
[Route("api/exam-templates")]
public class ExamTemplateController(IExamTemplateService service) : AuthorizeControllerBase
{
    /// <summary>Lấy mẫu đề thi theo ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ExamTemplateResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamTemplateResponse>.Success("Lấy dữ liệu thành công!", ExamTemplateResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy mẫu đề thi kèm phần thi</summary>
    [HttpGet("{id:guid}/with-sections")]
    public async Task<ActionResult<RequestResponse<ExamTemplateResponse>>> GetWithSections(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithSectionsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamTemplateResponse>.Success("Lấy dữ liệu thành công!", ExamTemplateResponse.FromEntity(result, includeSections: true), 1));
    }

    /// <summary>Lấy danh sách mẫu đề thi theo môn học</summary>
    [HttpGet("by-subject/{subjectId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamTemplateResponse>>>> GetBySubject(int subjectId, CancellationToken ct)
    {
        var result = await service.GetBySubjectAsync(subjectId, ct);
        var list = result.Select(t => ExamTemplateResponse.FromEntity(t)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamTemplateResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy danh sách mẫu đề thi theo lớp học</summary>
    [HttpGet("by-grade/{gradeLevelId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamTemplateResponse>>>> GetByGradeLevel(int gradeLevelId, CancellationToken ct)
    {
        var result = await service.GetByGradeLevelAsync(gradeLevelId, ct);
        var list = result.Select(t => ExamTemplateResponse.FromEntity(t)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamTemplateResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Tạo mẫu đề thi kèm phần thi</summary>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<ExamTemplateResponse>>> Create(
        [FromBody] ExamTemplateRequest request,
        CancellationToken ct)
    {
        var entity   = request.ToEntity(CurrentUser.UserName!);
        var sections = request.ToSections();
        var result   = await service.CreateAsync(entity, sections, ct);
        return StatusCode(201, RequestResponse<ExamTemplateResponse>.Success("Tạo mẫu đề thi thành công!", ExamTemplateResponse.FromEntity(result), 1));
    }

    /// <summary>Cập nhật mẫu đề thi (tuỳ chọn kèm phần thi mới)</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ExamTemplateResponse>>> Update(
        Guid id,
        [FromBody] ExamTemplateRequest request,
        CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var entity   = request.ToEntity(existing.CreatedBy!);
        entity.Id    = id;
        var sections = request.ToSections();
        var result   = await service.UpdateAsync(entity, sections, ct);
        return Ok(RequestResponse<ExamTemplateResponse>.Success("Cập nhật mẫu đề thi thành công!", ExamTemplateResponse.FromEntity(result), 1));
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
}
