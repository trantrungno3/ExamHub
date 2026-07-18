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

    /// <summary>Lấy câu hỏi theo ID (kèm đáp án)</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<QuestionResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithAnswersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<QuestionResponse>.Success("Lấy dữ liệu thành công!", QuestionResponse.FromEntity(result, includeAnswers: true), 1));
    }

    /// <summary>Lấy danh sách câu hỏi phân trang với bộ lọc</summary>
    [HttpGet]
    public async Task<ActionResult<RequestResponse<object>>> GetPaged([FromQuery] QuestionPagedRequest request, CancellationToken ct)
    {
        var (items, total) = await service.GetPagedAsync(
            request.Page, request.PageSize,
            request.TopicId, request.QuestionTypeId, request.DifficultyLevelId,
            request.CognitiveLevelId, request.Keyword, request.IsVerified, ct);

        return Ok(RequestResponse<object>.Success("Lấy danh sách thành công!", new
        {
            Total    = total,
            Page     = request.Page,
            PageSize = request.PageSize,
            Items    = items.Select(q => QuestionResponse.FromEntity(q)).ToList()
        }, total));
    }

    /// <summary>Lấy danh sách câu hỏi theo chủ đề</summary>
    [HttpGet("by-topic/{topicId:int}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<QuestionResponse>>>> GetByTopic(int topicId, CancellationToken ct)
    {
        var result = await service.GetByTopicAsync(topicId, ct);
        var list = result.Select(q => QuestionResponse.FromEntity(q)).ToList();
        return Ok(RequestResponse<IReadOnlyList<QuestionResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Tạo câu hỏi mới kèm đáp án</summary>
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
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Import câu hỏi hàng loạt từ file Excel (.xlsx)</summary>
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

    /// <summary>Tải lên tệp đính kèm (ảnh/PDF, tối đa 10 MB) cho câu hỏi → MinIO</summary>
    [HttpPost("{id:guid}/attachment")]
    public async Task<ActionResult<RequestResponse<object>>> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(RequestResponse<object>.Error("Tệp đính kèm không được để trống."));
        if (file.Length > MaxAttachmentBytes)
            return BadRequest(RequestResponse<object>.Error("Tệp vượt quá giới hạn 10 MB."));
        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(RequestResponse<object>.Error("Chỉ chấp nhận ảnh (jpeg/png/gif/webp) hoặc PDF."));

        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var topic = await topicRepo.GetByIdAsync(existing.TopicId, ct);
        if (topic is null)
            return NotFound(RequestResponse<object>.Error($"Chủ đề {existing.TopicId} không tồn tại."));

        var authResult = await authorizationService.AuthorizeAsync(User, topic.SubjectId, "TeacherOwnsSubject");
        if (!authResult.Succeeded)
            return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách môn học này."));

        var objectName = $"questions/{id}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        var (ok, url) = await storage.UploadStreamAsync(stream, objectName, file.ContentType);
        if (!ok || url is null)
            return StatusCode(500, RequestResponse<object>.Error("Tải tệp lên MinIO thất bại."));

        await service.SetImageUrlAsync(id, url, ct);
        return Ok(RequestResponse<object>.Success("Tải tệp đính kèm thành công!", new { Url = url }, 1));
    }

    /// <summary>Kiểm duyệt câu hỏi</summary>
    [HttpPost("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        await service.VerifyAsync(id, CurrentUser.UserId!.Value, ct);
        return NoContent();
    }
}
