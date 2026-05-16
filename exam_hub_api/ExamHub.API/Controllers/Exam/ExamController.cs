using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý đề thi</summary>
[ApiController]
[Authorize]
[Route("api/exams")]
public class ExamController(IExamService service) : ControllerBase
{
    /// <summary>Lấy đề thi theo ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExamResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(ExamResponse.FromEntity(result));
    }

    /// <summary>Lấy đề thi kèm câu hỏi snapshot</summary>
    [HttpGet("{id:guid}/with-questions")]
    public async Task<ActionResult<ExamResponse>> GetWithQuestions(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithQuestionsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(ExamResponse.FromEntity(result, includeQuestions: true));
    }

    /// <summary>Lấy danh sách đề thi phân trang với bộ lọc</summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetPaged([FromQuery] ExamPagedRequest request, CancellationToken ct)
    {
        var (items, total) = await service.GetPagedAsync(
            request.Page, request.PageSize,
            request.GradeLevelId, request.SubjectId,
            request.Status, request.Keyword, ct);

        return Ok(new
        {
            Total    = total,
            Page     = request.Page,
            PageSize = request.PageSize,
            Items    = items.Select(e => ExamResponse.FromEntity(e)).ToList()
        });
    }

    /// <summary>Lấy danh sách đề thi biến thể cùng lô</summary>
    [HttpGet("{parentId:guid}/variants")]
    public async Task<ActionResult<IReadOnlyList<ExamResponse>>> GetVariants(Guid parentId, CancellationToken ct)
    {
        var result = await service.GetVariantsAsync(parentId, ct);
        return Ok(result.Select(e => ExamResponse.FromEntity(e)).ToList());
    }

    /// <summary>Tạo đề thi kèm câu hỏi snapshot</summary>
    [HttpPost]
    public async Task<ActionResult<ExamResponse>> Create(
        [FromBody] ExamRequest request,
        CancellationToken ct)
    {
        var userId    = GetCurrentUserId();
        var entity    = request.ToEntity(userId);
        var questions = request.ToQuestions();
        var result    = await service.CreateAsync(entity, questions, ct);
        return StatusCode(201, ExamResponse.FromEntity(result));
    }

    /// <summary>Phát hành đề thi (Draft → Published)</summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<bool>> Publish(Guid id, CancellationToken ct)
    {
        var result = await service.PublishAsync(id, ct);
        if (!result) return NotFound();
        return Ok(result);
    }

    /// <summary>Lưu trữ đề thi (Published → Archived)</summary>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<bool>> Archive(Guid id, CancellationToken ct)
    {
        var result = await service.ArchiveAsync(id, ct);
        if (!result) return NotFound();
        return Ok(result);
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
