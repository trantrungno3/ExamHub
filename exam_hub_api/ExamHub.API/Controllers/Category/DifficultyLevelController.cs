using ExamHub.Core.DataTransferObjects.Category;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Category;

/// <summary>Controller quản lý mức độ khó</summary>
[ApiController]
[Route("api/[controller]")]
public class DifficultyLevelController(IDifficultyLevelService service)
    : CategoryBaseController<DifficultyLevel, int, DifficultyLevelRequest, DifficultyLevelResponse>(service)
{
    /// <inheritdoc/>
    protected override DifficultyLevel ToEntity(DifficultyLevelRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override DifficultyLevel ToEntityForUpdate(int id, DifficultyLevelRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override DifficultyLevelResponse ToResponse(DifficultyLevel entity) => DifficultyLevelResponse.FromEntity(entity);

    /// <summary>Lấy theo mã</summary>
    [HttpGet("code/{code}")]
    [Authorize]
    public async Task<ActionResult<DifficultyLevelResponse>> GetByCode(string code, CancellationToken ct = default)
    {
        var result = await service.GetByCodeAsync(code, ct);
        if (result is null) return NotFound();
        return Ok(ToResponse(result));
    }
}
