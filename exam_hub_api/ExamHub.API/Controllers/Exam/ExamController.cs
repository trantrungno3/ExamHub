using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý đề thi</summary>
[ApiController]
[Authorize]
[Route("api/exams")]
public class ExamController(IExamService service, IExportService exportService) : ControllerBase
{
    /// <summary>Lấy đề thi theo ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ExamResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamResponse>.Success("Lấy dữ liệu thành công!", ExamResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy đề thi kèm câu hỏi snapshot</summary>
    [HttpGet("{id:guid}/with-questions")]
    public async Task<ActionResult<RequestResponse<ExamResponse>>> GetWithQuestions(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithQuestionsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamResponse>.Success("Lấy dữ liệu thành công!", ExamResponse.FromEntity(result, includeQuestions: true), 1));
    }

    /// <summary>Lấy danh sách đề thi phân trang với bộ lọc</summary>
    [HttpGet]
    public async Task<ActionResult<RequestResponse<object>>> GetPaged([FromQuery] ExamPagedRequest request, CancellationToken ct)
    {
        var (items, total) = await service.GetPagedAsync(
            request.Page, request.PageSize,
            request.GradeLevelId, request.SubjectId,
            request.Status, request.Keyword, ct);

        return Ok(RequestResponse<object>.Success("Lấy danh sách thành công!", new
        {
            Total    = total,
            Page     = request.Page,
            PageSize = request.PageSize,
            Items    = items.Select(e => ExamResponse.FromEntity(e)).ToList()
        }, total));
    }

    /// <summary>Lấy danh sách đề thi biến thể cùng lô</summary>
    [HttpGet("{parentId:guid}/variants")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamResponse>>>> GetVariants(Guid parentId, CancellationToken ct)
    {
        var result = await service.GetVariantsAsync(parentId, ct);
        var list = result.Select(e => ExamResponse.FromEntity(e)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Tạo đề thi kèm câu hỏi snapshot</summary>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<ExamResponse>>> Create(
        [FromBody] ExamRequest request,
        CancellationToken ct)
    {
        var userId    = GetCurrentUserId();
        var entity    = request.ToEntity(userId);
        var questions = request.ToQuestions();
        var result    = await service.CreateAsync(entity, questions, ct);
        return StatusCode(201, RequestResponse<ExamResponse>.Success("Tạo đề thi thành công!", ExamResponse.FromEntity(result), 1));
    }

    /// <summary>Phát hành đề thi (Draft → Published)</summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<RequestResponse<bool>>> Publish(Guid id, CancellationToken ct)
    {
        var result = await service.PublishAsync(id, ct);
        if (!result) return NotFound();
        return Ok(RequestResponse<bool>.Success("Phát hành đề thi thành công!", result, 1));
    }

    /// <summary>Lưu trữ đề thi (Published → Archived)</summary>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<RequestResponse<bool>>> Archive(Guid id, CancellationToken ct)
    {
        var result = await service.ArchiveAsync(id, ct);
        if (!result) return NotFound();
        return Ok(RequestResponse<bool>.Success("Lưu trữ đề thi thành công!", result, 1));
    }

    /// <summary>Thống kê phân bổ câu hỏi trong đề thi (Bloom / độ khó / chủ đề)</summary>
    [HttpGet("{id:guid}/analytics")]
    public async Task<ActionResult<RequestResponse<ExamAnalyticsResponse>>> GetAnalytics(Guid id, CancellationToken ct)
    {
        var result = await service.GetAnalyticsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamAnalyticsResponse>.Success("Lấy thống kê thành công!", result, result.TotalQuestions));
    }

    /// <summary>Xuất đề thi ra PDF / Word, lưu lên MinIO và trả về URL tải về</summary>
    [HttpGet("{id:guid}/export")]
    public async Task<ActionResult<RequestResponse<object>>> Export(
        Guid id, [FromQuery] string format, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var fmt = (format ?? "pdf").Trim().ToLowerInvariant();
        var url = fmt switch
        {
            "pdf"  => await exportService.ExportPdfAsync(id, ct),
            "docx" => await exportService.ExportDocxAsync(id, ct),
            _      => null
        };
        if (url is null)
            return BadRequest(RequestResponse<object>.Error("Định dạng không hợp lệ. Chỉ hỗ trợ 'pdf' hoặc 'docx'."));

        return Ok(RequestResponse<object>.Success("Xuất đề thi thành công!", new { Url = url, Format = fmt }, 1));
    }

    /// <summary>Xóa đề thi</summary>
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
