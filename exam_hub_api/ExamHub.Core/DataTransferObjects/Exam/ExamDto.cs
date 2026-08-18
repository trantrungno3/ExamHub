using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.Exam;

// ── ExamQuestion ──────────────────────────────────────────────────────────────

/// <summary>Request DTO cho một câu hỏi snapshot trong đề thi</summary>
public record ExamQuestionRequest(
    Guid QuestionId,
    string? SectionName,
    decimal? Score,
    string ContentSnapshot,
    string? AnswersSnapshot
)
{
    /// <summary>Map sang entity</summary>
    public ExamQuestion ToEntity() => new()
    {
        QuestionId       = QuestionId,
        SectionName      = SectionName,
        Score            = Score,
        ContentSnapshot  = ContentSnapshot,
        AnswersSnapshot  = AnswersSnapshot
    };
}

/// <summary>Response DTO cho một câu hỏi snapshot</summary>
public record ExamQuestionResponse(
    Guid Id,
    Guid QuestionId,
    string? SectionName,
    int SortOrder,
    decimal? Score,
    string ContentSnapshot,
    string? AnswersSnapshot,
    // Media lấy từ câu hỏi gốc (không snapshot) — ImageUrl có thể là ảnh hoặc pdf.
    string? ImageUrl,
    string? AudioUrl
)
{
    /// <summary>Map từ entity</summary>
    public static ExamQuestionResponse FromEntity(ExamQuestion e) =>
        new(e.Id, e.QuestionId, e.SectionName, e.SortOrder, e.Score, e.ContentSnapshot, e.AnswersSnapshot,
            e.Question?.ImageUrl, e.Question?.AudioUrl);
}

// ── Exam ──────────────────────────────────────────────────────────────────────

/// <summary>Request DTO để tạo đề thi</summary>
public record ExamRequest(
    Guid? ExamTemplateId,
    int GradeLevelId,
    int SubjectId,
    string Title,
    string? ExamCode,
    int DurationMinutes,
    decimal TotalScore,
    string? Instructions,
    string? SchoolYear,
    short? Semester,
    DateOnly? ExamDate,
    string? ClassName,
    Guid? ParentExamId,
    short? VariantIndex,
    Guid? BatchId,
    IEnumerable<ExamQuestionRequest>? Questions
)
{
    /// <summary>Map sang entity</summary>
    public Domain.Entities.Exam ToEntity(string createdBy) => new()
    {
        ExamTemplateId   = ExamTemplateId,
        GradeLevelId     = GradeLevelId,
        SubjectId        = SubjectId,
        CreatedBy        = createdBy,
        Title            = Title,
        ExamCode         = ExamCode,
        DurationMinutes  = DurationMinutes,
        TotalScore       = TotalScore,
        Instructions     = Instructions,
        SchoolYear       = SchoolYear,
        Semester         = Semester,
        ExamDate         = ExamDate,
        ClassName        = ClassName,
        ParentExamId     = ParentExamId,
        VariantIndex     = VariantIndex,
        BatchId          = BatchId
    };

    /// <summary>Map danh sách câu hỏi</summary>
    public IEnumerable<ExamQuestion> ToQuestions() =>
        Questions?.Select(q => q.ToEntity()) ?? [];
}

/// <summary>Response DTO cho đề thi</summary>
public record ExamResponse(
    Guid Id,
    Guid? ExamTemplateId,
    int GradeLevelId,
    string? GradeLevelName,
    int SubjectId,
    string? SubjectName,
    string Title,
    string? ExamCode,
    int DurationMinutes,
    decimal TotalScore,
    string? Instructions,
    string Status,
    string? SchoolYear,
    short? Semester,
    DateOnly? ExamDate,
    string? ClassName,
    Guid? ParentExamId,
    short? VariantIndex,
    Guid? BatchId,
    long Created,
    long Modified,
    IReadOnlyList<ExamQuestionResponse>? Questions
)
{
    /// <summary>Map từ entity</summary>
    public static ExamResponse FromEntity(Domain.Entities.Exam e, bool includeQuestions = false) =>
        new(
            e.Id, e.ExamTemplateId,
            e.GradeLevelId, e.GradeLevel?.Name,
            e.SubjectId,    e.Subject?.Name,
            e.Title, e.ExamCode,
            e.DurationMinutes, e.TotalScore, e.Instructions,
            e.Status.ToString(),
            e.SchoolYear, e.Semester, e.ExamDate, e.ClassName,
            e.ParentExamId, e.VariantIndex, e.BatchId,
            e.Created.ToTimestamp(),
            e.Modified.ToTimestamp(),
            includeQuestions ? e.Questions.Select(ExamQuestionResponse.FromEntity).ToList() : null
        );
}

/// <summary>Request lọc đề thi phân trang</summary>
public record ExamPagedRequest(
    int Page = 1,
    int PageSize = 20,
    int? GradeLevelId = null,
    int? SubjectId = null,
    ExamStatusEnum? Status = null,
    string? Keyword = null
);
