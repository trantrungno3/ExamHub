using Microsoft.AspNetCore.RateLimiting;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using TVT.Core.MinioStorage;

namespace ExamHub.API.Controllers.Question;

/// <summary>Controller quản lý ngân hàng câu hỏi</summary>
[ApiController]
[Route("api/questions")]
public class QuestionController(
    IQuestionService service,
    IAuthorizationService authorizationService,
    ITopicRepository topicRepo,
    IMinioStorageService storage,
    IBulkImportService bulkImportService) : AuthorizeControllerBase
{
    private const long MaxAttachmentBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"];
    private static readonly string[] AllowedAudioTypes =
        ["audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/ogg", "audio/webm", "audio/mp4"];

    /// <summary>Lấy câu hỏi theo ID (kèm đáp án)</summary>
    /// <param name="id">Id câu hỏi cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Câu hỏi kèm đáp án; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<QuestionResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithAnswersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<QuestionResponse>.Success("Lấy dữ liệu thành công!", QuestionResponse.FromEntity(result, includeAnswers: true), 1));
    }

    /// <summary>Lấy danh sách câu hỏi phân trang với bộ lọc</summary>
    /// <param name="request">Tham số phân trang và bộ lọc (chủ đề, loại câu hỏi, độ khó, mức nhận thức, từ khoá, trạng thái duyệt).</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách câu hỏi đã lọc kèm tổng số bản ghi.</returns>
    [HttpGet]
    public async Task<ActionResult<RequestResponse<object>>> GetPaged([FromQuery] QuestionPagedRequest request, CancellationToken ct)
    {
        var (items, total) = await service.GetPagedAsync(
            request.Page, request.PageSize,
            request.TopicId, request.QuestionTypeId, request.DifficultyLevelId,
            request.CognitiveLevelId, request.Keyword, request.ReviewStatus,
            request.SubjectId, request.GradeLevelId, ct);

        return Ok(RequestResponse<object>.Success("Lấy danh sách thành công!", new
        {
            Total    = total,
            Page     = request.Page,
            PageSize = request.PageSize,
            Items    = items.Select(q => QuestionResponse.FromEntity(q)).ToList()
        }, total));
    }

    /// <summary>Lấy danh sách câu hỏi theo chủ đề</summary>
    /// <param name="topicId">Id chủ đề cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách câu hỏi thuộc chủ đề.</returns>
    [HttpGet("by-topic/{topicId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<QuestionResponse>>>> GetByTopic(int topicId, CancellationToken ct)
    {
        var result = await service.GetByTopicAsync(topicId, ct);
        var list = result.Select(q => QuestionResponse.FromEntity(q)).ToList();
        return Ok(RequestResponse<IReadOnlyList<QuestionResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Tạo câu hỏi mới kèm đáp án</summary>
    /// <param name="request">Nội dung câu hỏi và danh sách đáp án.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Câu hỏi vừa tạo (HTTP 201); 403 nếu không phụ trách môn học của chủ đề.</returns>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<QuestionResponse>>> Create(
        [FromBody] QuestionRequest request,
        CancellationToken ct)
    {
        var topic = await topicRepo.GetByIdAsync(request.TopicId, ct);
        if (topic is null)
            return NotFound(RequestResponse<object>.Error($"Chủ đề {request.TopicId} không tồn tại."));

        var authResult = await authorizationService.AuthorizeAsync(User, topic.SubjectId, "TeacherOwnsSubject");
        if (!authResult.Succeeded)
            return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách môn học này."));

        var entity  = request.ToEntity(CurrentUser.UserName!);
        var answers = request.ToAnswers();
        var result  = await service.CreateAsync(entity, answers, ct);
        return StatusCode(201, RequestResponse<QuestionResponse>.Success("Tạo câu hỏi thành công!", QuestionResponse.FromEntity(result), 1));
    }

    /// <summary>Cập nhật câu hỏi (tuỳ chọn kèm đáp án mới)</summary>
    /// <param name="id">Id câu hỏi cần cập nhật.</param>
    /// <param name="request">Nội dung câu hỏi mới và danh sách đáp án.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Câu hỏi sau khi cập nhật; 404 nếu không tồn tại; 403 nếu không phụ trách môn học.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequestResponse<QuestionResponse>>> Update(
        Guid id,
        [FromBody] QuestionRequest request,
        CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var topic = await topicRepo.GetByIdAsync(request.TopicId, ct);
        if (topic is null)
            return NotFound(RequestResponse<object>.Error($"Chủ đề {request.TopicId} không tồn tại."));

        var authResult = await authorizationService.AuthorizeAsync(User, topic.SubjectId, "TeacherOwnsSubject");
        if (!authResult.Succeeded)
            return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách môn học này."));

        var entity  = request.ToEntity(existing.CreatedBy!);
        entity.Id   = id;
        var answers = request.ToAnswers();
        var result  = await service.UpdateAsync(entity, answers, ct);
        return Ok(RequestResponse<QuestionResponse>.Success("Cập nhật câu hỏi thành công!", QuestionResponse.FromEntity(result), 1));
    }

    /// <summary>Xóa câu hỏi</summary>
    /// <param name="id">Id câu hỏi cần xoá.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi xoá thành công; 404 nếu không tồn tại; 409 nếu đang được tham chiếu (đã dùng trong đề thi).</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        try
        {
            await service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (EntityInUseException ex)
        {
            return Conflict(RequestResponse<object>.Error(ex.Message));
        }
    }

    /// <summary>Import câu hỏi hàng loạt từ file Excel (.xlsx)</summary>
    /// <param name="request">File Excel chứa câu hỏi + đáp án cần import.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả import kèm số dòng thành công/lỗi.</returns>
    [EnableRateLimiting("write-heavy")]
    [HttpPost("bulk-import")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<BulkImportQuestionResponse>>> BulkImport(
        [FromForm] BulkImportQuestionRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(RequestResponse<object>.Error("File import không được để trống."));

        var result = await bulkImportService.ImportAsync(request, CurrentUser.UserName, ct);
        return Ok(RequestResponse<BulkImportQuestionResponse>.Success(
            $"Import hoàn tất: {result.SuccessCount} thành công, {result.ErrorCount} lỗi.", result, result.SuccessCount));
    }

    /// <summary>
    /// Tải tệp đính kèm (ảnh/PDF, ≤ 10 MB) lên MinIO → trả URL. KHÔNG ghi DB;
    /// URL chỉ được lưu khi Save câu hỏi (client gửi kèm <c>ImageUrl</c> trong QuestionRequest).
    /// Cho phép upload trước cả khi câu hỏi được tạo (không cần id).
    /// </summary>
    /// <param name="file">Tệp ảnh/PDF cần upload, tối đa 10 MB.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>URL của tệp trên MinIO.</returns>
    [EnableRateLimiting("write-heavy")]
    [HttpPost("attachment")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<object>>> UploadAttachment(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(RequestResponse<object>.Error("Tệp đính kèm không được để trống."));
        if (file.Length > MaxAttachmentBytes)
            return BadRequest(RequestResponse<object>.Error("Tệp vượt quá giới hạn 10 MB."));
        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(RequestResponse<object>.Error("Chỉ chấp nhận ảnh (jpeg/png/gif/webp) hoặc PDF."));

        var objectName = $"questions/_uploads/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        var (ok, url) = await storage.UploadStreamAsync(stream, objectName, file.ContentType);
        if (!ok || url is null)
            return StatusCode(500, RequestResponse<object>.Error("Tải tệp lên MinIO thất bại."));

        return Ok(RequestResponse<object>.Success("Tải tệp đính kèm thành công!", new { Url = url }, 1));
    }

    /// <summary>
    /// Tải tệp audio (≤ 10 MB) lên MinIO → trả URL. KHÔNG ghi DB;
    /// URL chỉ được lưu khi Save câu hỏi (client gửi kèm <c>AudioUrl</c> trong QuestionRequest).
    /// </summary>
    /// <param name="file">Tệp audio cần upload, tối đa 10 MB.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>URL của tệp trên MinIO.</returns>
    [EnableRateLimiting("write-heavy")]
    [HttpPost("audio")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<object>>> UploadAudio(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(RequestResponse<object>.Error("Tệp audio không được để trống."));
        if (file.Length > MaxAttachmentBytes)
            return BadRequest(RequestResponse<object>.Error("Tệp vượt quá giới hạn 10 MB."));
        if (!AllowedAudioTypes.Contains(file.ContentType))
            return BadRequest(RequestResponse<object>.Error("Chỉ chấp nhận tệp audio (mp3/wav/ogg/webm/mp4)."));

        var objectName = $"questions/_uploads/audio/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        var (ok, url) = await storage.UploadStreamAsync(stream, objectName, file.ContentType);
        if (!ok || url is null)
            return StatusCode(500, RequestResponse<object>.Error("Tải audio lên MinIO thất bại."));

        return Ok(RequestResponse<object>.Success("Tải audio thành công!", new { Url = url }, 1));
    }

    /// <summary>Kiểm duyệt câu hỏi</summary>
    /// <param name="id">Id câu hỏi cần duyệt.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi duyệt thành công; 404 nếu không tồn tại.</returns>
    [HttpPost("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.VerifyAsync(id, CurrentUser.UserId!.Value, ct);
        return NoContent();
    }

    /// <summary>Bỏ duyệt câu hỏi</summary>
    /// <param name="id">Id câu hỏi cần bỏ duyệt.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi thành công; 404 nếu không tồn tại.</returns>
    [HttpPost("{id:guid}/unverify")]
    public async Task<IActionResult> Unverify(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.UnverifyAsync(id, ct);
        return NoContent();
    }

    /// <summary>Từ chối câu hỏi kèm lý do</summary>
    /// <param name="id">Id câu hỏi cần từ chối.</param>
    /// <param name="req">Lý do từ chối.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi thành công; 400 nếu thiếu lý do; 404 nếu không tồn tại.</returns>
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<RequestResponse<object>>> Reject(Guid id, [FromBody] RejectQuestionRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Reason))
            return BadRequest(RequestResponse<object>.Error("Vui lòng nhập lý do từ chối."));
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.RejectAsync(id, CurrentUser.UserId!.Value, req.Reason.Trim(), ct);
        return NoContent();
    }

    /// <summary>Thống kê số câu hỏi theo trạng thái</summary>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Số lượng câu hỏi theo từng trạng thái duyệt.</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<RequestResponse<QuestionStatsResponse>>> GetStats(CancellationToken ct)
    {
        var stats = await service.GetStatsAsync(ct);
        return Ok(RequestResponse<QuestionStatsResponse>.Success("Lấy thống kê thành công!", stats, 1));
    }
}
