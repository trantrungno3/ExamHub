using Microsoft.AspNetCore.RateLimiting;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using TVT.Core.Enums;
using TVT.Core.Extensions;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý kỳ thi (exam sessions) và luồng làm bài của học sinh.</summary>
[ApiController]
[Route("api/exam-sessions")]
public class ExamSessionController(IExamSessionService service, IAuthorizationService authorizationService) : AuthorizeControllerBase
{
    // ── Quản lý (Admin/Teacher) ─────────────────────────────────────────
    /// <summary>Danh sách kỳ thi phân trang.</summary>
    /// <param name="page">Số trang (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số bản ghi mỗi trang.</param>
    /// <param name="subjectId">Lọc theo môn học (tuỳ chọn).</param>
    /// <param name="gradeLevelId">Lọc theo khối lớp (tuỳ chọn).</param>
    /// <param name="status">Lọc theo trạng thái kỳ thi (tuỳ chọn).</param>
    /// <param name="keyword">Từ khoá tìm theo tên kỳ thi (tuỳ chọn).</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách kỳ thi đã lọc kèm tổng số bản ghi.</returns>
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
    /// <param name="id">Id kỳ thi cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Chi tiết kỳ thi; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<ExamSessionDetailResponse>>> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await service.GetDetailAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamSessionDetailResponse>.Success("Lấy dữ liệu thành công!", result, 1));
    }

    /// <summary>Tạo kỳ thi.</summary>
    /// <param name="request">Cấu hình kỳ thi cần tạo.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Id kỳ thi vừa tạo (HTTP 201).</returns>
    [HttpPost, Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<Guid>>> Create([FromBody] CreateExamSessionRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, User.GetTag(), ct);
        return result.Status == RequestResponseStatus.Success ? StatusCode(201, result) : Ok(result);
    }

    /// <summary>Cập nhật kỳ thi.</summary>
    /// <param name="id">Id kỳ thi cần cập nhật.</param>
    /// <param name="request">Cấu hình kỳ thi mới.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả cập nhật.</returns>
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Update(Guid id, [FromBody] UpdateExamSessionRequest request, CancellationToken ct)
    {
        return Ok(await service.UpdateAsync(id, request, User.GetTag(), ct));
    }

    /// <summary>Xoá kỳ thi.</summary>
    /// <param name="id">Id kỳ thi cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả xoá.</returns>
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Ok(RequestResponse<bool>.Success("Xoá kỳ thi thành công!", true, 1));
    }

    /// <summary>Đặt/thêm đề vào pool của kỳ thi.</summary>
    /// <param name="id">Id kỳ thi cần cập nhật pool.</param>
    /// <param name="request">Danh sách Id đề thi cần đặt vào pool.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả cập nhật.</returns>
    [HttpPost("{id:guid}/exams"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> SetExams(Guid id, [FromBody] SetSessionExamsRequest request, CancellationToken ct)
    {
        return Ok(await service.SetExamsAsync(id, request.ExamIds, User.GetTag(), ct));
    }

    /// <summary>Gỡ một đề khỏi pool.</summary>
    /// <param name="id">Id kỳ thi.</param>
    /// <param name="examId">Id đề thi cần gỡ khỏi pool.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả gỡ đề.</returns>
    [HttpDelete("{id:guid}/exams/{examId:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> RemoveExam(Guid id, Guid examId, CancellationToken ct)
    {
        await service.RemoveExamAsync(id, examId, ct);
        return Ok(RequestResponse<bool>.Success("Gỡ đề thành công!", true, 1));
    }

    /// <summary>Giao kỳ thi cho một lớp/khoá.</summary>
    /// <param name="id">Id kỳ thi cần giao.</param>
    /// <param name="request">Lớp (CohortClassId) hoặc cả khoá cần giao; giao cả khoá chỉ Admin được phép.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Id assignment vừa tạo (HTTP 201); 403 nếu không đủ quyền trên lớp/khoá.</returns>
    [HttpPost("{id:guid}/assignments"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<Guid>>> AddAssignment(Guid id, [FromBody] CreateAssignmentRequest request, CancellationToken ct)
    {
        if (request.CohortClassId is { } cohortClassId)
        {
            var authResult = await authorizationService.AuthorizeAsync(User, cohortClassId, "TeacherOwnsCohortClass");
            if (!authResult.Succeeded)
                return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách lớp học này."));
        }
        else if (!User.IsInRole("Admin"))
        {
            return StatusCode(403, RequestResponse<object>.Error("Chỉ Quản trị viên được giao kỳ thi cho cả khoá."));
        }

        var result = await service.AddAssignmentAsync(id, request, ct);
        return result.Status == RequestResponseStatus.Success ? StatusCode(201, result) : Ok(result);
    }

    /// <summary>Gỡ giao lớp/khoá.</summary>
    /// <param name="id">Id kỳ thi.</param>
    /// <param name="assignmentId">Id assignment cần gỡ.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả gỡ; 404 nếu assignment không tồn tại; 403 nếu không đủ quyền trên lớp/khoá.</returns>
    [HttpDelete("{id:guid}/assignments/{assignmentId:guid}"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> RemoveAssignment(Guid id, Guid assignmentId, CancellationToken ct)
    {
        var assignment = await service.GetAssignmentByIdAsync(assignmentId, ct);
        if (assignment is null) return NotFound();

        if (assignment.CohortClassId is { } cohortClassId)
        {
            var authResult = await authorizationService.AuthorizeAsync(User, cohortClassId, "TeacherOwnsCohortClass");
            if (!authResult.Succeeded)
                return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách lớp học này."));
        }
        else if (!User.IsInRole("Admin"))
        {
            return StatusCode(403, RequestResponse<object>.Error("Chỉ Quản trị viên được gỡ giao cả khoá."));
        }

        await service.RemoveAssignmentAsync(assignmentId, ct);
        return Ok(RequestResponse<bool>.Success("Gỡ giao thành công!", true, 1));
    }

    /// <summary>Phát hành kỳ thi (Draft → Published).</summary>
    /// <param name="id">Id kỳ thi cần phát hành.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả phát hành.</returns>
    [HttpPost("{id:guid}/publish"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Publish(Guid id, CancellationToken ct)
    {
        return Ok(await service.PublishAsync(id, ct));
    }

    /// <summary>Đóng kỳ thi.</summary>
    /// <param name="id">Id kỳ thi cần đóng.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả đóng kỳ thi.</returns>
    [HttpPost("{id:guid}/close"), Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<bool>>> Close(Guid id, CancellationToken ct)
    {
        await service.CloseAsync(id, ct);
        return Ok(RequestResponse<bool>.Success("Đóng kỳ thi thành công!", true, 1));
    }

    // ── Học sinh ────────────────────────────────────────────────────────
    /// <summary>Danh sách kỳ thi được giao cho học sinh hiện tại.</summary>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách kỳ thi được giao; 401 nếu không xác định được người dùng hiện tại.</returns>
    [HttpGet("my")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<MySessionResponse>>>> GetMy(CancellationToken ct)
    {
        if (CurrentUser.UserId.IsNullOrEmpty())
            return StatusCode(401, RequestResponse<IReadOnlyList<MySessionResponse>>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
        var result = await service.GetMySessionsAsync(CurrentUser.UserId!.Value, ct);
        return Ok(RequestResponse<IReadOnlyList<MySessionResponse>>.Success("Lấy danh sách thành công!", result, result.Count));
    }

    /// <summary>Pool đề của kỳ thi kèm trạng thái làm bài của học sinh hiện tại.</summary>
    /// <param name="id">Id kỳ thi cần lấy pool đề.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách đề trong pool kèm trạng thái làm bài; 401 nếu không xác định được người dùng hiện tại.</returns>
    [HttpGet("{id:guid}/pool")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<SessionPoolItemResponse>>>> GetPool(Guid id, CancellationToken ct)
    {
        if (CurrentUser.UserId.IsNullOrEmpty())
            return StatusCode(401, RequestResponse<IReadOnlyList<SessionPoolItemResponse>>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
        var result = await service.GetPoolForStudentAsync(id, CurrentUser.UserId!.Value, ct);
        return Ok(result);
    }

    /// <summary>Vào thi: bốc/khoá đề (Random) hoặc chọn đề (StudentChoice), trả submission + đề.</summary>
    /// <param name="id">Id kỳ thi cần bắt đầu làm bài.</param>
    /// <param name="request">Đề thi được chọn (chỉ dùng khi PickMode=StudentChoice).</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Submission vừa tạo kèm đề thi; 401 nếu không xác định được người dùng hiện tại.</returns>
    [EnableRateLimiting("write-heavy")]
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<RequestResponse<StartSessionResponse>>> Start(Guid id, [FromBody] StartSessionRequest request, CancellationToken ct)
    {
        if (CurrentUser.UserId.IsNullOrEmpty())
            return StatusCode(401, RequestResponse<StartSessionResponse>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
        return Ok(await service.StartAsync(id, CurrentUser.UserId!.Value, request.ExamId, User.GetTag(), ct));
    }
}
