using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;
using TVT.Core.Extensions;

namespace ExamHub.API.Controllers.Exam;

/// <summary>Controller quản lý bài nộp thi</summary>
[ApiController]
[Route("api/exam-submissions")]
public class ExamSubmissionController(IExamSubmissionService service) : AuthorizeControllerBase
{
    /// <summary>Lấy bài nộp theo ID (kèm câu trả lời)</summary>
    /// <param name="id">Id bài nộp cần lấy.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bài nộp kèm câu trả lời; 404 nếu không tồn tại.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ExamSubmissionResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetWithAnswersAsync(id, ct);
        if (result is null) return NotFound();
        var directory = await service.GetStudentDirectoryAsync(new List<Guid> { result.StudentId }, ct);
        directory.TryGetValue(result.StudentId, out var info);
        return Ok(RequestResponse<ExamSubmissionResponse>.Success(
            "Lấy dữ liệu thành công!",
            ExamSubmissionResponse.FromEntity(result, info?.Name, info?.ClassName, includeAnswers: true), 1));
    }

    /// <summary>Lấy danh sách bài nộp theo đề thi</summary>
    /// <param name="examId">Id đề thi cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách bài nộp thuộc đề thi.</returns>
    [HttpGet("by-exam/{examId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamSubmissionResponse>>>> GetByExam(Guid examId, CancellationToken ct)
    {
        var result = await service.GetByExamAsync(examId, ct);
        var list = result.Select(s => ExamSubmissionResponse.FromEntity(s)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamSubmissionResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy tất cả bài nộp của một học sinh</summary>
    /// <param name="studentId">Id học sinh cần tra cứu.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách bài nộp của học sinh.</returns>
    [HttpGet("by-student/{studentId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamSubmissionResponse>>>> GetByStudent(Guid studentId, CancellationToken ct)
    {
        var result = await service.GetByStudentAsync(studentId, ct);
        var list = result.Select(s => ExamSubmissionResponse.FromEntity(s)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamSubmissionResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy bài nộp của học sinh theo đề thi</summary>
    /// <param name="examId">Id đề thi.</param>
    /// <param name="studentId">Id học sinh.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bài nộp tương ứng; 404 nếu không tồn tại.</returns>
    [HttpGet("by-exam/{examId:guid}/student/{studentId:guid}")]
    public async Task<ActionResult<RequestResponse<ExamSubmissionResponse>>> GetByExamAndStudent(
        Guid examId, Guid studentId, CancellationToken ct)
    {
        var result = await service.GetByExamAndStudentAsync(examId, studentId, ct);
        if (result is null) return NotFound();
        return Ok(RequestResponse<ExamSubmissionResponse>.Success("Lấy dữ liệu thành công!", ExamSubmissionResponse.FromEntity(result), 1));
    }

    /// <summary>Lấy danh sách bài nộp theo kỳ thi (giáo viên chấm bài)</summary>
    /// <param name="sessionId">Id kỳ thi cần lọc.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách bài nộp thuộc kỳ thi, kèm tên/lớp học sinh.</returns>
    [HttpGet("by-session/{sessionId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamSubmissionResponse>>>> GetBySession(Guid sessionId, CancellationToken ct)
    {
        var result = await service.GetBySessionAsync(sessionId, ct);
        var directory = await service.GetStudentDirectoryAsync(
            result.Select(s => s.StudentId).Distinct().ToList(), ct);
        var list = result.Select(s =>
        {
            directory.TryGetValue(s.StudentId, out var info);
            return ExamSubmissionResponse.FromEntity(s, info?.Name, info?.ClassName);
        }).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamSubmissionResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Lấy các lần nộp của một học sinh trong một kỳ thi (học sinh xem lại kết quả)</summary>
    /// <param name="sessionId">Id kỳ thi.</param>
    /// <param name="studentId">Id học sinh.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Danh sách các lần nộp bài của học sinh trong kỳ thi.</returns>
    [HttpGet("by-session/{sessionId:guid}/student/{studentId:guid}")]
    public async Task<ActionResult<RequestResponse<IReadOnlyList<ExamSubmissionResponse>>>> GetBySessionAndStudent(
        Guid sessionId, Guid studentId, CancellationToken ct)
    {
        var result = await service.GetBySessionAndStudentAsync(sessionId, studentId, ct);
        var list = result.Select(s => ExamSubmissionResponse.FromEntity(s)).ToList();
        return Ok(RequestResponse<IReadOnlyList<ExamSubmissionResponse>>.Success("Lấy danh sách thành công!", list, list.Count));
    }

    /// <summary>Nộp bài thi kèm câu trả lời</summary>
    /// <param name="request">Bài làm và danh sách câu trả lời cần nộp.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bài nộp vừa tạo (HTTP 201).</returns>
    [HttpPost]
    public async Task<ActionResult<RequestResponse<ExamSubmissionResponse>>> Submit(
        [FromBody] ExamSubmissionRequest request,
        CancellationToken ct)
    {
        var submission = request.ToEntity();
        var answers    = request.ToAnswers();
        submission.CreatedBy = User.GetTag();
        var result     = await service.SubmitAsync(submission, answers, ct);
        return StatusCode(201, RequestResponse<ExamSubmissionResponse>.Success("Nộp bài thành công!", ExamSubmissionResponse.FromEntity(result), 1));
    }

    /// <summary>Chấm điểm câu tự luận</summary>
    /// <param name="answerId">Id câu trả lời cần chấm.</param>
    /// <param name="request">Điểm, đúng/sai và nhận xét cho câu trả lời.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>204 khi chấm thành công.</returns>
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

    /// <summary>Chốt điểm bài nộp: tổng hợp điểm từng câu và chuyển trạng thái sang Graded</summary>
    /// <param name="id">Id bài nộp cần chốt điểm.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Bài nộp sau khi chốt điểm.</returns>
    [HttpPost("{id:guid}/finalize")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<RequestResponse<ExamSubmissionResponse>>> Finalize(Guid id, CancellationToken ct)
    {
        var result = await service.FinalizeAsync(id, CurrentUser.UserId!.Value, ct);
        return Ok(RequestResponse<ExamSubmissionResponse>.Success("Chốt điểm thành công!", ExamSubmissionResponse.FromEntity(result), 1));
    }

    /// <summary>Lưu tạm bài làm (autosave) cho bản InProgress — không đổi trạng thái, không chấm.</summary>
    /// <param name="id">Id bài nộp (InProgress) cần lưu tạm.</param>
    /// <param name="answers">Danh sách câu trả lời hiện tại của học sinh.</param>
    /// <param name="ct">Token huỷ yêu cầu.</param>
    /// <returns>Kết quả lưu tạm; 403 nếu không phải chủ bài nộp; 409 nếu bài nộp không còn ở trạng thái InProgress.</returns>
    [HttpPut("{id:guid}/progress")]
    public async Task<ActionResult<RequestResponse<bool>>> SaveProgress(
        Guid id, [FromBody] IEnumerable<SubmissionAnswerRequest> answers, CancellationToken ct)
    {
        try
        {
            await service.SaveProgressAsync(
                id,
                CurrentUser.UserId!.Value,
                (answers ?? Enumerable.Empty<SubmissionAnswerRequest>()).Select(a => a.ToEntity()),
                ct);
            return Ok(RequestResponse<bool>.Success("Đã lưu tạm bài làm.", true, 1));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, RequestResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(RequestResponse<object>.Error(ex.Message));
        }
    }
}
