using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using TVT.Core.Extensions;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý kỳ thi (exam sessions) và luồng làm bài của học sinh.</summary>
[ApiController]
[Route("api/exam-sessions")]
public class ExamSessionController(IExamSessionService service) : AuthorizeControllerBase
{
    // ── Quản lý (Admin/Teacher) ─────────────────────────────────────────
    /// <summary>Danh sách kỳ thi phân trang.</summary>
    [HttpGet, Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<object>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] int? subjectId = null, [FromQuery] int? gradeLevelId = null,
        [FromQuery] ExamSessionStatusEnum? status = null, [FromQuery] string? keyword = null,
        CancellationToken ct = default)
    {
        var (items, total) = await service.GetPagedAsync(page, pageSize, subjectId, gradeLevelId, status, keyword, ct);
        return Ok(RequestResponse<object>.Success("Lấy danh sách thành công!", new
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items
        }, total));
    }

    /// <summary>Chi tiết kỳ thi kèm pool đề + assignments.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<ExamSessionDetailResponse>>> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await service.GetDetailAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamSessionDetailResponse>.Success("Lấy dữ liệu thành công!", result, 1));
    }

    /// <summary>Tạo kỳ thi.</summary>
    [HttpPost, Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<Guid>>> Create([FromBody] CreateExamSessionRequest request, CancellationToken ct)
    {
        var id = await service.CreateAsync(request, User.GetTag(), ct);
        return StatusCode(201, RequestResponse<Guid>.Success("Tạo kỳ thi thành công!", id, 1));
    }

    /// <summary>Cập nhật kỳ thi.</summary>
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Update(Guid id, [FromBody] UpdateExamSessionRequest request, CancellationToken ct)
    {
        await service.UpdateAsync(id, request, User.GetTag(), ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật kỳ thi thành công!", true, 1));
    }

    /// <summary>Xoá kỳ thi.</summary>
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Ok(RequestResponse<bool>.Success("Xoá kỳ thi thành công!", true, 1));
    }

    /// <summary>Đặt/thêm đề vào pool của kỳ thi.</summary>
    [HttpPost("{id:guid}/exams"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> SetExams(Guid id, [FromBody] SetSessionExamsRequest request, CancellationToken ct)
    {
        await service.SetExamsAsync(id, request.ExamIds, User.GetTag(), ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật đề thi thành công!", true, 1));
    }

    /// <summary>Gỡ một đề khỏi pool.</summary>
    [HttpDelete("{id:guid}/exams/{examId:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> RemoveExam(Guid id, Guid examId, CancellationToken ct)
    {
        await service.RemoveExamAsync(id, examId, ct);
        return Ok(RequestResponse<bool>.Success("Gỡ đề thành công!", true, 1));
    }

    /// <summary>Giao kỳ thi cho một lớp/khoá.</summary>
    [HttpPost("{id:guid}/assignments"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<Guid>>> AddAssignment(Guid id, [FromBody] CreateAssignmentRequest request, CancellationToken ct)
    {
        var assignmentId = await service.AddAssignmentAsync(id, request, ct);
        return StatusCode(201, RequestResponse<Guid>.Success("Giao kỳ thi thành công!", assignmentId, 1));
    }

    /// <summary>Gỡ giao lớp/khoá.</summary>
    [HttpDelete("{id:guid}/assignments/{assignmentId:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> RemoveAssignment(Guid id, Guid assignmentId, CancellationToken ct)
    {
        await service.RemoveAssignmentAsync(assignmentId, ct);
        return Ok(RequestResponse<bool>.Success("Gỡ giao thành công!", true, 1));
    }

    /// <summary>Phát hành kỳ thi (Draft → Published).</summary>
    [HttpPost("{id:guid}/publish"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Publish(Guid id, CancellationToken ct)
    {
        await service.PublishAsync(id, ct);
        return Ok(RequestResponse<bool>.Success("Phát hành kỳ thi thành công!", true, 1));
    }

    /// <summary>Đóng kỳ thi.</summary>
    [HttpPost("{id:guid}/close"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Close(Guid id, CancellationToken ct)
    {
        await service.CloseAsync(id, ct);
        return Ok(RequestResponse<bool>.Success("Đóng kỳ thi thành công!", true, 1));
    }

    // ── Học sinh ────────────────────────────────────────────────────────
    /// <summary>Danh sách kỳ thi được giao cho học sinh hiện tại.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<MySessionResponse>>>> GetMy(CancellationToken ct)
    {
        if (CurrentUser.UserId.IsNullOrEmpty())
            return StatusCode(401, RequestResponse<IReadOnlyList<MySessionResponse>>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
        var result = await service.GetMySessionsAsync(CurrentUser.UserId!.Value, ct);
        return Ok(RequestResponse<IReadOnlyList<MySessionResponse>>.Success("Lấy danh sách thành công!", result, result.Count));
    }

    /// <summary>Pool đề của kỳ thi kèm trạng thái làm bài của học sinh hiện tại.</summary>
    [HttpGet("{id:guid}/pool")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<SessionPoolItemResponse>>>> GetPool(Guid id, CancellationToken ct)
    {
        if (CurrentUser.UserId.IsNullOrEmpty())
            return StatusCode(401, RequestResponse<IReadOnlyList<SessionPoolItemResponse>>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
        var result = await service.GetPoolForStudentAsync(id, CurrentUser.UserId!.Value, ct);
        return Ok(RequestResponse<IReadOnlyList<SessionPoolItemResponse>>.Success("Lấy danh sách thành công!", result, result.Count));
    }

    /// <summary>Vào thi: bốc/khoá đề (Random) hoặc chọn đề (StudentChoice), trả submission + đề.</summary>
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<RequestResponse<StartSessionResponse>>> Start(Guid id, [FromBody] StartSessionRequest request, CancellationToken ct)
    {
        if (CurrentUser.UserId.IsNullOrEmpty())
            return StatusCode(401, RequestResponse<StartSessionResponse>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
        var result = await service.StartAsync(id, CurrentUser.UserId!.Value, request.ExamId, User.GetTag(), ct);
        return Ok(RequestResponse<StartSessionResponse>.Success("Vào thi thành công!", result, 1));
    }
}
