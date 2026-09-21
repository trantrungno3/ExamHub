using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVT.Core;

namespace ExamHub.API.Controllers;

/// <summary>
/// Base controller CRUD tái sử dụng cho các entity dạng danh mục với DTO request/response.
/// </summary>
/// <typeparam name="TEntity">Kiểu entity danh mục</typeparam>
/// <typeparam name="TKey">Kiểu khóa chính</typeparam>
/// <typeparam name="TRequest">Kiểu DTO request (create/update)</typeparam>
/// <typeparam name="TResponse">Kiểu DTO response</typeparam>
[ApiController]
public abstract class CategoryBaseController<TEntity, TKey, TRequest, TResponse>(
    ICategoryService<TEntity, TKey> service) : AuthorizeControllerBase
    where TEntity : class
{
    /// <summary>Map request DTO → entity (dùng cho Create)</summary>
    /// <param name="request">DTO request nhận từ client.</param>
    /// <returns>Entity mới tạo từ request, chưa lưu DB.</returns>
    protected abstract TEntity ToEntity(TRequest request);

    /// <summary>Map request DTO + id → entity (dùng cho Update)</summary>
    /// <param name="id">Khoá chính của entity cần cập nhật.</param>
    /// <param name="request">DTO request nhận từ client.</param>
    /// <returns>Entity đã gán id, sẵn sàng truyền cho <c>UpdateAsync</c>.</returns>
    protected abstract TEntity ToEntityForUpdate(TKey id, TRequest request);

    /// <summary>Map entity → response DTO</summary>
    /// <param name="entity">Entity lấy từ service/DB.</param>
    /// <returns>DTO response trả về client.</returns>
    protected abstract TResponse ToResponse(TEntity entity);

    /// <summary>Lấy toàn bộ danh sách</summary>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Toàn bộ danh sách, không phân biệt trạng thái kích hoạt.</returns>
    [HttpGet("")]
    public virtual async Task<ActionResult<RequestResponse<IReadOnlyList<TResponse>>>> GetAll(CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(ct);
        var list = result.Select(ToResponse).ToList();
        return Ok(RequestResponse<IReadOnlyList<TResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy danh sách đang kích hoạt</summary>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách các bản ghi có <c>IsActive = true</c>.</returns>
    [HttpGet("active")]
    public virtual async Task<ActionResult<RequestResponse<IReadOnlyList<TResponse>>>> GetActive(CancellationToken ct = default)
    {
        var result = await service.GetActiveAsync(ct);
        var list = result.Select(ToResponse).ToList();
        return Ok(RequestResponse<IReadOnlyList<TResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy theo ID</summary>
    /// <param name="id">Khoá chính cần tra cứu.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<RequestResponse<TResponse>>> GetById(TKey id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<TResponse>.Success("Lấy dữ liệu thành công!", ToResponse(result), 1));
    }

    /// <summary>Tạo mới</summary>
    /// <param name="request">DTO chứa dữ liệu bản ghi cần tạo.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi vừa tạo (HTTP 201).</returns>
    [HttpPost("")]
    public virtual async Task<ActionResult<RequestResponse<TResponse>>> Create([FromBody] TRequest request, CancellationToken ct = default)
    {
        var entity = ToEntity(request);
        var result = await service.CreateAsync(entity, ct);
        return StatusCode(201, RequestResponse<TResponse>.Success("Tạo mới thành công!", ToResponse(result), 1));
    }

    /// <summary>Cập nhật</summary>
    /// <param name="id">Khoá chính của bản ghi cần cập nhật.</param>
    /// <param name="request">DTO chứa dữ liệu mới.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bản ghi sau khi cập nhật; 404 nếu không tồn tại.</returns>
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<RequestResponse<TResponse>>> Update(TKey id, [FromBody] TRequest request, CancellationToken ct = default)
    {
        if (!await service.ExistsAsync(id, ct)) return NotFound();
        var entity = ToEntityForUpdate(id, request);
        var result = await service.UpdateAsync(entity, ct);
        return Ok(RequestResponse<TResponse>.Success("Cập nhật thành công!", ToResponse(result), 1));
    }

    /// <summary>Xóa theo ID</summary>
    /// <param name="id">Khoá chính của bản ghi cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 nếu xoá thành công; 409 nếu bản ghi đang được tham chiếu ở nơi khác.</returns>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(TKey id, CancellationToken ct = default)
    {
        try
        {
            await service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (ExamHub.Core.Application.Services.EntityInUseException ex)
        {
            return Conflict(RequestResponse<object>.Error(ex.Message));
        }
        catch (DbUpdateException)
        {
            // Backstop cho các tham chiếu khoá ngoại KHÔNG được service kiểm tra tường minh
            // (vd. cohort_classes.grade_level_id, exam_templates/exam_sessions.subject_id,
            // exam_template_sections.topic_id/question_type_id...). Không có middleware xử lý
            // exception toàn cục trong codebase này, nên nếu không bắt ở đây sẽ lộ 500 thay vì
            // 409 tiếng Việt như UC18 yêu cầu. Cùng pattern với UserController (task D5).
            return Conflict(RequestResponse<object>.Error(
                "Dữ liệu đang được sử dụng ở nơi khác, không thể xoá."));
        }
    }

    /// <summary>Bật/tắt kích hoạt</summary>
    /// <param name="id">Khoá chính của bản ghi cần đổi trạng thái.</param>
    /// <param name="isActive">Trạng thái kích hoạt mới.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Trạng thái kích hoạt sau khi cập nhật.</returns>
    [HttpPatch("{id}/active")]
    public virtual async Task<ActionResult<RequestResponse<bool>>> SetActive(TKey id, [FromBody] bool isActive, CancellationToken ct = default)
    {
        var result = await service.SetActiveAsync(id, isActive, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật trạng thái thành công!", result, 1));
    }
}
