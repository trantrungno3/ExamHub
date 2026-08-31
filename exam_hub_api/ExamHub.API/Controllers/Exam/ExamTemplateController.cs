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
    /// <summary>Thống kê mẫu đề thi (stat card)</summary>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Số liệu thống kê tổng quan về mẫu đề thi.</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<RequestResponse<ExamTemplateStatsResponse>>> GetStats(CancellationToken ct)
    {
        var s = await service.GetStatsAsync(ct);
        return Ok(RequestResponse<ExamTemplateStatsResponse>.Success("Lấy thống kê thành công!", s, 1));
    }

    /// <summary>Lấy mẫu đề thi theo ID</summary>
    /// <param name="id">Id mẫu đề thi cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Mẫu đề thi (không kèm phần thi); 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ExamTemplateResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamTemplateResponse>.Success("Lấy dữ liệu thành công!", ExamTemplateResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy mẫu đề thi kèm phần thi</summary>
    /// <param name="id">Id mẫu đề thi cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Mẫu đề thi kèm danh sách phần thi; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}/with-sections")]
    public async Task<ActionResult<RequestResponse<ExamTemplateResponse>>> GetWithSections(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithSectionsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamTemplateResponse>.Success("Lấy dữ liệu thành công!", ExamTemplateResponse.FromEntity(result, includeSections: true), 1));
    }

    /// <summary>Lấy danh sách mẫu đề thi theo môn học</summary>
    /// <param name="subjectId">Id môn học cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách mẫu đề thi thuộc môn học.</returns>
    [HttpGet("by-subject/{subjectId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamTemplateResponse>>>> GetBySubject(int subjectId, CancellationToken ct)
    {
        var result = await service.GetBySubjectAsync(subjectId, ct);
        var list = result.Select(t => ExamTemplateResponse.FromEntity(t)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamTemplateResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy danh sách mẫu đề thi theo lớp học</summary>
    /// <param name="gradeLevelId">Id khối lớp cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách mẫu đề thi thuộc khối lớp.</returns>
    [HttpGet("by-grade/{gradeLevelId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamTemplateResponse>>>> GetByGradeLevel(int gradeLevelId, CancellationToken ct)
    {
        var result = await service.GetByGradeLevelAsync(gradeLevelId, ct);
        var list = result.Select(t => ExamTemplateResponse.FromEntity(t)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamTemplateResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Tạo mẫu đề thi kèm phần thi</summary>
    /// <param name="request">Thông tin mẫu đề thi và danh sách phần thi.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Mẫu đề thi vừa tạo (HTTP 201).</returns>
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
    /// <param name="id">Id mẫu đề thi cần cập nhật.</param>
    /// <param name="request">Thông tin mẫu đề thi mới và danh sách phần thi.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Mẫu đề thi sau khi cập nhật; 404 nếu không tồn tại.</returns>
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
    /// <param name="id">Id mẫu đề thi cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công; 404 nếu không tồn tại.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
