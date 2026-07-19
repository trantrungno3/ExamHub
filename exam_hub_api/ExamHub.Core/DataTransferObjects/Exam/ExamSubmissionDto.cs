using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.Exam;

// ── SubmissionAnswer ──────────────────────────────────────────────────────────

/// <summary>Request DTO cho một câu trả lời khi nộp bài</summary>
public record SubmissionAnswerRequest(
    Guid ExamQuestionId,
    Guid[]? SelectedAnswerIds,
    string? EssayContent
)
{
    /// <summary>Map sang entity</summary>
    public SubmissionAnswer ToEntity() => new()
    {
        ExamQuestionId    = ExamQuestionId,
        SelectedAnswerIds = SelectedAnswerIds,
        EssayContent      = EssayContent
    };
}

/// <summary>Response DTO cho một câu trả lời</summary>
public record SubmissionAnswerResponse(
    Guid Id,
    Guid ExamQuestionId,
    Guid[]? SelectedAnswerIds,
    string? EssayContent,
    bool? IsCorrect,
    decimal ScoreEarned,
    string? Feedback,
    Guid? GradedBy
)
{
    /// <summary>Map từ entity</summary>
    public static SubmissionAnswerResponse FromEntity(SubmissionAnswer e) =>
        new(e.Id, e.ExamQuestionId, e.SelectedAnswerIds, e.EssayContent,
            e.IsCorrect, e.ScoreEarned, e.Feedback, e.GradedBy);
}

// ── ExamSubmission ────────────────────────────────────────────────────────────

/// <summary>Request DTO nộp bài thi</summary>
public record ExamSubmissionRequest(
    Guid ExamId,
    Guid StudentId,
    IEnumerable<SubmissionAnswerRequest> Answers,
    Guid? SessionId = null,
    Guid? SubmissionId = null
)
{
    /// <summary>Map sang entity bài nộp. Khi có <see cref="SubmissionId"/> (luồng kỳ thi) — gán Id để service cập nhật bản in_progress thay vì tạo mới.</summary>
    public ExamSubmission ToEntity() => new()
    {
        Id        = SubmissionId ?? Guid.Empty,
        ExamId    = ExamId,
        StudentId = StudentId,
        SessionId = SessionId,
        StartedAt = DateTime.UtcNow
    };

    /// <summary>Map danh sách câu trả lời</summary>
    public IEnumerable<SubmissionAnswer> ToAnswers() =>
        Answers.Select(a => a.ToEntity());
}

/// <summary>Request DTO chấm điểm câu tự luận</summary>
public record GradeAnswerRequest(
    decimal ScoreEarned,
    bool IsCorrect,
    string? Feedback,
    Guid GradedBy
);

/// <summary>Response DTO cho bài nộp</summary>
public record ExamSubmissionResponse(
    Guid Id,
    Guid ExamId,
    Guid StudentId,
    long StartedAt,
    long? SubmittedAt,
    int? DurationSeconds,
    decimal? TotalScore,
    bool? IsPassed,
    string Status,
    long Created,
    IReadOnlyList<SubmissionAnswerResponse>? Answers
)
{
    /// <summary>Map từ entity</summary>
    public static ExamSubmissionResponse FromEntity(ExamSubmission e, bool includeAnswers = false) =>
        new(
            e.Id, e.ExamId, e.StudentId,
            e.StartedAt.ToTimestamp(),
            e.SubmittedAt?.ToTimestamp(),
            e.DurationSeconds,
            e.TotalScore,
            e.IsPassed,
            e.Status.ToString(),
            e.Created.ToTimestamp(),
            includeAnswers ? e.Answers.Select(SubmissionAnswerResponse.FromEntity).ToList() : null
        );
}
