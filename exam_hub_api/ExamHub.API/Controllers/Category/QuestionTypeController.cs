using ExamHub.Core.DataTransferObjects.Category;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Category;

/// <summary>Controller quản lý loại câu hỏi</summary>
[ApiController]
[Route("api/[controller]")]
public class QuestionTypeController(IQuestionTypeService service)
    : CategoryBaseController<QuestionType, int, QuestionTypeRequest, QuestionTypeResponse>(service)
{
    /// <inheritdoc/>
    protected override QuestionType ToEntity(QuestionTypeRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override QuestionType ToEntityForUpdate(int id, QuestionTypeRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override QuestionTypeResponse ToResponse(QuestionType entity) => QuestionTypeResponse.FromEntity(entity);

    /// <summary>Lấy theo mã</summary>
    [HttpGet("code/{code}")]
    [Authorize]
    public async Task<ActionResult<QuestionTypeResponse>> GetByCode(string code, CancellationToken ct = default)
    {
        var result = await service.GetByCodeAsync(code, ct);
        if (result is null) return NotFound();
        return Ok(ToResponse(result));
    }
}
