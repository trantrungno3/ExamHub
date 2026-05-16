using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Question;

/// <summary>Controller quản lý ngân hàng câu hỏi</summary>
[ApiController]
[Authorize]
[Route("api/questions")]
public class QuestionController(IQuestionService service) : ControllerBase
{
    /// <summary>Lấy câu hỏi theo ID (kèm đáp án)</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithAnswersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(QuestionResponse.FromEntity(result, includeAnswers: true));
    }

    /// <summary>Lấy danh sách câu hỏi phân trang với bộ lọc</summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetPaged([FromQuery] QuestionPagedRequest request, CancellationToken ct)
    {
        var (items, total) = await service.GetPagedAsync(
            request.Page, request.PageSize,
            request.TopicId, request.QuestionTypeId, request.DifficultyLevelId,
            request.Keyword, request.IsVerified, ct);

        return Ok(new
        {
            Total = total,
            Page  = request.Page,
            PageSize = request.PageSize,
            Items = items.Select(q => QuestionResponse.FromEntity(q)).ToList()
        });
    }

    /// <summary>Lấy danh sách câu hỏi theo chủ đề</summary>
    [HttpGet("by-topic/{topicId:int}")]
    public async Task<ActionResult<IReadOnlyList<QuestionResponse>>> GetByTopic(int topicId, CancellationToken ct)
    {
        var result = await service.GetByTopicAsync(topicId, ct);
        return Ok(result.Select(q => QuestionResponse.FromEntity(q)).ToList());
    }

    /// <summary>Tạo câu hỏi mới kèm đáp án</summary>
    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> Create(
        [FromBody] QuestionRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var entity  = request.ToEntity(userId);
        var answers = request.ToAnswers();
        var result  = await service.CreateAsync(entity, answers, ct);
        return StatusCode(201, QuestionResponse.FromEntity(result));
    }

    /// <summary>Cập nhật câu hỏi (tuỳ chọn kèm đáp án mới)</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuestionResponse>> Update(
        Guid id,
        [FromBody] QuestionRequest request,
        CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var entity  = request.ToEntity(existing.CreatedBy);
        entity.Id   = id;
        var answers = request.ToAnswers();
        var result  = await service.UpdateAsync(entity, answers, ct);
        return Ok(QuestionResponse.FromEntity(result));
    }

    /// <summary>Xóa câu hỏi</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Kiểm duyệt câu hỏi</summary>
    [HttpPost("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.VerifyAsync(id, GetCurrentUserId(), ct);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}
