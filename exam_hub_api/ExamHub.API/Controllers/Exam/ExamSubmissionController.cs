using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý bài nộp thi</summary>
[ApiController]
[Authorize]
[Route("api/exam-submissions")]
public class ExamSubmissionController(IExamSubmissionService service) : ControllerBase
{
    /// <summary>Lấy bài nộp theo ID (kèm câu trả lời)</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExamSubmissionResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithAnswersAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(ExamSubmissionResponse.FromEntity(result, includeAnswers: true));
    }

    /// <summary>Lấy danh sách bài nộp theo đề thi</summary>
    [HttpGet("by-exam/{examId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ExamSubmissionResponse>>> GetByExam(Guid examId, CancellationToken ct)
    {
        var result = await service.GetByExamAsync(examId, ct);
        return Ok(result.Select(s => ExamSubmissionResponse.FromEntity(s)).ToList());
    }

    /// <summary>Lấy tất cả bài nộp của một học sinh</summary>
    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ExamSubmissionResponse>>> GetByStudent(Guid studentId, CancellationToken ct)
    {
        var result = await service.GetByStudentAsync(studentId, ct);
        return Ok(result.Select(s => ExamSubmissionResponse.FromEntity(s)).ToList());
    }

    /// <summary>Lấy bài nộp của học sinh theo đề thi</summary>
    [HttpGet("by-exam/{examId:guid}/student/{studentId:guid}")]
    public async Task<ActionResult<ExamSubmissionResponse>> GetByExamAndStudent(
        Guid examId, Guid studentId, CancellationToken ct)
    {
        var result = await service.GetByExamAndStudentAsync(examId, studentId, ct);
        if (result is null) return NotFound();
        return Ok(ExamSubmissionResponse.FromEntity(result));
    }

    /// <summary>Nộp bài thi kèm câu trả lời</summary>
    [HttpPost]
    public async Task<ActionResult<ExamSubmissionResponse>> Submit(
        [FromBody] ExamSubmissionRequest request,
        CancellationToken ct)
    {
        var submission = request.ToEntity();
        var answers    = request.ToAnswers();
        var result     = await service.SubmitAsync(submission, answers, ct);
        return StatusCode(201, ExamSubmissionResponse.FromEntity(result));
    }

    /// <summary>Chấm điểm câu tự luận</summary>
    [HttpPost("answers/{answerId:guid}/grade")]
    public async Task<IActionResult> GradeAnswer(
        Guid answerId,
        [FromBody] GradeAnswerRequest request,
        CancellationToken ct)
    {
        await service.GradeAnswerAsync(
            answerId,
            request.ScoreEarned,
            request.IsCorrect,
            request.Feedback,
            request.GradedBy,
            ct);
        return NoContent();
    }
}
