using ExamHub.Core.DataTransferObjects.Category;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.Category;

/// <summary>Controller quản lý mức độ nhận thức (Bloom's Taxonomy)</summary>
[ApiController]
[Route("api/[controller]")]
public class CognitiveLevelController(ICognitiveLevelService service)
    : CategoryBaseController<CognitiveLevel, int, CognitiveLevelRequest, CognitiveLevelResponse>(service)
{
    /// <inheritdoc/>
    protected override CognitiveLevel ToEntity(CognitiveLevelRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override CognitiveLevel ToEntityForUpdate(int id, CognitiveLevelRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override CognitiveLevelResponse ToResponse(CognitiveLevel entity) => CognitiveLevelResponse.FromEntity(entity);

    /// <summary>Lấy theo mã (remember, understand, apply, ...)</summary>
    [HttpGet("code/{code}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<CognitiveLevelResponse>>> GetByCode(string code, CancellationToken ct = default)
    {
        var result = await service.GetByCodeAsync(code, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<CognitiveLevelResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }
}
