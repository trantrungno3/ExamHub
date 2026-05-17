using ExamHub.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller sinh đề thi tự động từ ngân hàng câu hỏi</summary>
[ApiController]
[Authorize]
[Route("api/exam-generator")]
public class ExamGeneratorController(IExamGeneratorService service) : ControllerBase
{
    /// <summary>Sinh đề thi theo cấu hình phần thi và tỉ lệ độ khó</summary>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<object>>> Generate(
        [FromBody] GenerateExamApiRequest request,
        CancellationToken ct)
    {
        var examId = await service.GenerateAsync(request.ToServiceRequest(GetCurrentUserId()), ct);
        return StatusCode(201, RequestResponse<object>.Success("Sinh đề thi thành công!", new { ExamId = examId }, 1));
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}

/// <summary>Request DTO để sinh đề thi qua API — CreatedBy được lấy từ JWT.</summary>
public sealed record GenerateExamApiRequest(
    string Title,
    Guid? ExamTemplateId,
    int GradeLevelId,
    int SubjectId,
    int DurationMinutes,
    bool ShuffleQuestions,
    IReadOnlyList<SectionConfig> Sections)
{
    /// <summary>Chuyển đổi sang service request với CreatedBy từ JWT.</summary>
    public GenerateExamRequest ToServiceRequest(Guid createdBy) => new(
        Title, ExamTemplateId, GradeLevelId, SubjectId,
        DurationMinutes, ShuffleQuestions, createdBy, Sections);
}
