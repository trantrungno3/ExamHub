using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers;

/// <summary>
/// Base controller CRUD tái sử dụng cho các entity dạng danh mục với DTO request/response.
/// </summary>
/// <typeparam name="TEntity">Kiểu entity danh mục</typeparam>
/// <typeparam name="TKey">Kiểu khóa chính</typeparam>
/// <typeparam name="TRequest">Kiểu DTO request (create/update)</typeparam>
/// <typeparam name="TResponse">Kiểu DTO response</typeparam>
[Authorize]
[ApiController]
public abstract class CategoryBaseController<TEntity, TKey, TRequest, TResponse>(
    ICategoryService<TEntity, TKey> service) : ControllerBase
    where TEntity : class
{
    /// <summary>Map request DTO → entity (dùng cho Create)</summary>
    protected abstract TEntity ToEntity(TRequest request);

    /// <summary>Map request DTO + id → entity (dùng cho Update)</summary>
    protected abstract TEntity ToEntityForUpdate(TKey id, TRequest request);

    /// <summary>Map entity → response DTO</summary>
    protected abstract TResponse ToResponse(TEntity entity);

    /// <summary>Lấy toàn bộ danh sách</summary>
    [HttpGet("")]
    public virtual async Task<ActionResult<RequestResponse<IReadOnlyList<TResponse>>>> GetAll(CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(ct);
        var list = result.Select(ToResponse).ToList();
        return Ok(RequestResponse<IReadOnlyList<TResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy danh sách đang kích hoạt</summary>
    [HttpGet("active")]
    public virtual async Task<ActionResult<RequestResponse<IReadOnlyList<TResponse>>>> GetActive(CancellationToken ct = default)
    {
        var result = await service.GetActiveAsync(ct);
        var list = result.Select(ToResponse).ToList();
        return Ok(RequestResponse<IReadOnlyList<TResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy theo ID</summary>
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<RequestResponse<TResponse>>> GetById(TKey id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<TResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Tạo mới</summary>
    [HttpPost("")]
    public virtual async Task<ActionResult<RequestResponse<TResponse>>> Create([FromBody] TRequest request, CancellationToken ct = default)
    {
        var entity = ToEntity(request);
        var result = await service.CreateAsync(entity, ct);
        return StatusCode(201, RequestResponse<TResponse>.Success("Tạo mới thành công!", ToResponse(result), 1));
    }

    /// <summary>Cập nhật</summary>
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<RequestResponse<TResponse>>> Update(TKey id, [FromBody] TRequest request, CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct)) return NotFound();
        var entity = ToEntityForUpdate(id, request);
        var result = await service.UpdateAsync(entity, ct);
        return Ok(RequestResponse<TResponse>.Success("Cập nhật thành công!", ToResponse(result), 1));
    }

    /// <summary>Xóa theo ID</summary>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(TKey id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Bật/tắt kích hoạt</summary>
    [HttpPatch("{id}/active")]
    public virtual async Task<ActionResult<RequestResponse<bool>>> SetActive(TKey id, [FromBody] bool isActive, CancellationToken ct = default)
    {
        var result = await service.SetActiveAsync(id, isActive, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật trạng thái thành công!", result, 1));
    }
}
