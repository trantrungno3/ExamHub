using ExamHub.Core.DataTransferObjects.Category;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

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
    /// <param name="code">Mã loại câu hỏi cần tra cứu.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("code/{code}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<QuestionTypeResponse>>> GetByCode(string code, CancellationToken ct = default)
    {
        var result = await service.GetByCodeAsync(code, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<QuestionTypeResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }
}
