using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Exam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller sinh đề thi tự động từ ngân hàng câu hỏi</summary>
[ApiController]
[Authorize]
[Route("api/exam-generator")]
public class ExamGeneratorController(
    IExamGeneratorService service,
    IAuthorizationService authorizationService) : ControllerBase
{
    /// <summary>Sinh đề thi theo cấu hình phần thi và tỉ lệ độ khó</summary>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<object>>> Generate(
        [FromBody] GenerateExamApiRequest request,
        CancellationToken ct)
    {
        try
        {
            var authResult = await authorizationService.AuthorizeAsync(User, request.SubjectId, "TeacherOwnsSubject");
            if (!authResult.Succeeded)
                return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách môn học này."));

            var examId = await service.GenerateAsync(request.ToServiceRequest(GetCurrentUserId()), ct);
            return StatusCode(201, RequestResponse<object>.Success("Sinh đề thi thành công!", new { ExamId = examId }, 1));
        }
        catch (InsufficientQuestionsException ex)
        {
            return BadRequest(RequestResponse<object>.Error(ex.Message));
        }
    }

    /// <summary>Sinh lô đề thi nhiều biến thể từ cùng ngân hàng câu hỏi</summary>
    [HttpPost("batch")]
    public async Task<ActionResult<RequestResponse<BatchGenerateExamResponse>>> BatchGenerate(
        [FromBody] BatchGenerateExamApiRequest request,
        CancellationToken ct)
    {
        try
        {
            var authResult = await authorizationService.AuthorizeAsync(User, request.SubjectId, "TeacherOwnsSubject");
            if (!authResult.Succeeded)
                return StatusCode(403, RequestResponse<object>.Error("Bạn không phụ trách môn học này."));

            var result = await service.BatchGenerateAsync(request.ToServiceRequest(GetCurrentUserId()), ct);
            var response = new BatchGenerateExamResponse(
                result.BatchId,
                result.Variants.Select(v =>
                    new VariantSummaryResponse(v.ExamId, v.ExamCode, v.VariantIndex, v.VariantCode)).ToList());
            return StatusCode(201, RequestResponse<BatchGenerateExamResponse>.Success(
                $"Đã sinh {result.Variants.Count} biến thể thành công!", response, result.Variants.Count));
        }
        catch (InsufficientQuestionsException ex)
        {
            return BadRequest(RequestResponse<object>.Error(ex.Message));
        }
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}