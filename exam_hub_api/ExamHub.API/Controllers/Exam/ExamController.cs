using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using TVT.Core.Extensions;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý đề thi</summary>
[ApiController]
[Route("api/exams")]
public class ExamController(IExamService service, IExportService exportService) : AuthorizeControllerBase
{
    /// <summary>Lấy đề thi theo ID</summary>
    /// <param name="id">Id đề thi cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Đề thi (không kèm danh sách câu hỏi); 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ExamResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamResponse>.Success("Lấy dữ liệu thành công!", ExamResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy đề thi kèm câu hỏi snapshot</summary>
    /// <param name="id">Id đề thi cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Đề thi kèm danh sách câu hỏi snapshot; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}/with-questions")]
    public async Task<ActionResult<RequestResponse<ExamResponse>>> GetWithQuestions(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithQuestionsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamResponse>.Success("Lấy dữ liệu thành công!", ExamResponse.FromEntity(result, includeQuestions: true), 1));
    }

    /// <summary>Lấy danh sách đề thi phân trang với bộ lọc</summary>
    /// <param name="request">Tham số phân trang và bộ lọc (khối lớp, môn học, trạng thái, từ khoá).</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách đề thi đã lọc kèm tổng số bản ghi.</returns>
    [HttpGet]
    public async Task<ActionResult<RequestResponse<object>>> GetPaged([FromQuery] ExamPagedRequest request, CancellationToken ct)
    {
        var (items, total) = await service.GetPagedAsync(
            request.Page, request.PageSize,
            request.GradeLevelId, request.SubjectId,
            request.Status, request.Keyword, ct);

        return Ok(RequestResponse<object>.Success("Lấy danh sách thành công!", new
        {
            Total    = total,
            Page     = request.Page,
            PageSize = request.PageSize,
            Items    = items.Select(e => ExamResponse.FromEntity(e)).ToList()
        }, total));
    }

    /// <summary>Lấy danh sách đề thi biến thể cùng lô</summary>
    /// <param name="parentId">Id đề thi gốc/lô cần lấy biến thể.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách các biến thể cùng lô.</returns>
    [HttpGet("{parentId:guid}/variants")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamResponse>>>> GetVariants(Guid parentId, CancellationToken ct)
    {
        var result = await service.GetVariantsAsync(parentId, ct);
        var list = result.Select(e => ExamResponse.FromEntity(e)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Tạo đề thi kèm câu hỏi snapshot</summary>
    /// <param name="request">Thông tin đề thi và danh sách câu hỏi cần snapshot.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Đề thi vừa tạo (HTTP 201); 401 nếu không xác định được người dùng hiện tại.</returns>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<ExamResponse>>> Create(
        [FromBody] ExamRequest request,
        CancellationToken ct)
    {
        if(CurrentUser.UserId.IsNullOrEmpty())
            return StatusCode(401, RequestResponse<ExamResponse>.Error("Không xác định được danh tính người dùng. Vui lòng đăng nhập lại."));
        var entity    = request.ToEntity(CurrentUser.UserName!);
        var questions = request.ToQuestions();
        var result    = await service.CreateAsync(entity, questions, ct);
        return StatusCode(201, RequestResponse<ExamResponse>.Success("Tạo đề thi thành công!", ExamResponse.FromEntity(result), 1));
    }

    /// <summary>Phát hành đề thi (Draft → Published)</summary>
    /// <param name="id">Id đề thi cần phát hành.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả phát hành; 404 nếu không tồn tại.</returns>
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<RequestResponse<bool>>> Publish(Guid id, CancellationToken ct)
    {
        var result = await service.PublishAsync(id, ct);
        if (!result) return NotFound();
        return Ok(RequestResponse<bool>.Success("Phát hành đề thi thành công!", result, 1));
    }

    /// <summary>Lưu trữ đề thi (Published → Archived)</summary>
    /// <param name="id">Id đề thi cần lưu trữ.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả lưu trữ; 404 nếu không tồn tại.</returns>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<RequestResponse<bool>>> Archive(Guid id, CancellationToken ct)
    {
        var result = await service.ArchiveAsync(id, ct);
        if (!result) return NotFound();
        return Ok(RequestResponse<bool>.Success("Lưu trữ đề thi thành công!", result, 1));
    }

    /// <summary>Thống kê phân bổ câu hỏi trong đề thi (Bloom / độ khó / chủ đề)</summary>
    /// <param name="id">Id đề thi cần thống kê.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Số liệu phân bổ câu hỏi theo mức nhận thức/độ khó/chủ đề; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}/analytics")]
    public async Task<ActionResult<RequestResponse<ExamAnalyticsResponse>>> GetAnalytics(Guid id, CancellationToken ct)
    {
        var result = await service.GetAnalyticsAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamAnalyticsResponse>.Success("Lấy thống kê thành công!", result, result.TotalQuestions));
    }

    /// <summary>Xuất đề thi ra PDF / Word, lưu lên MinIO và trả về URL tải về</summary>
    /// <param name="id">Id đề thi cần xuất.</param>
    /// <param name="format">Định dạng xuất: "pdf" hoặc "docx".</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>URL tệp đã xuất; 404 nếu đề thi không tồn tại; 400 nếu định dạng không hợp lệ.</returns>
    [HttpGet("{id:guid}/export")]
    public async Task<ActionResult<RequestResponse<object>>> Export(
        Guid id, [FromQuery] string format, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var fmt = (format ?? "pdf").Trim().ToLowerInvariant();
        var result = fmt switch
        {
            "pdf"  => await exportService.ExportPdfAsync(id, ct),
            "docx" => await exportService.ExportDocxAsync(id, ct),
            _      => RequestResponse<string>.Error("Định dạng không hợp lệ. Chỉ hỗ trợ 'pdf' hoặc 'docx'.")
        };
        if (result.Status == TVT.Core.Enums.RequestResponseStatus.Error)
            return Ok(RequestResponse<object>.Error(result.Message!));

        return Ok(RequestResponse<object>.Success("Xuất đề thi thành công!", new { Url = result.Data, Format = fmt }, 1));
    }

    /// <summary>Xóa đề thi</summary>
    /// <param name="id">Id đề thi cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công; 404 nếu không tồn tại.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
