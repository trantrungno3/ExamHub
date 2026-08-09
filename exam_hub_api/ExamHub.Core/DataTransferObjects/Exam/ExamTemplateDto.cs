using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.Exam;

// ── Section ──────────────────────────────────────────────────────────────────

/// <summary>Request DTO cho một phần trong mẫu đề thi</summary>
public record ExamTemplateSectionRequest(
    int? TopicId,
    int? QuestionTypeId,
    int? CognitiveLevelId,
    string? SectionName,
    int QuestionCount,
    decimal? ScorePerQuestion,
    short PctEasy,
    short PctMedium,
    short PctHard,
    short PctVeryHard
)
{
    /// <summary>Map sang entity</summary>
    public ExamTemplateSection ToEntity() => new()
    {
        TopicId          = TopicId,
        QuestionTypeId   = QuestionTypeId,
        CognitiveLevelId = CognitiveLevelId,
        SectionName      = SectionName,
        QuestionCount    = QuestionCount,
        ScorePerQuestion = ScorePerQuestion,
        PctEasy          = PctEasy,
        PctMedium        = PctMedium,
        PctHard          = PctHard,
        PctVeryHard      = PctVeryHard
    };
}

/// <summary>Response DTO cho một phần trong mẫu đề thi</summary>
public record ExamTemplateSectionResponse(
    Guid Id,
    Guid ExamTemplateId,
    int? TopicId,
    string? TopicName,
    int? QuestionTypeId,
    string? QuestionTypeName,
    int? CognitiveLevelId,
    string? CognitiveLevelName,
    string? SectionName,
    int QuestionCount,
    decimal? ScorePerQuestion,
    short SortOrder,
    short PctEasy,
    short PctMedium,
    short PctHard,
    short PctVeryHard
)
{
    /// <summary>Map từ entity</summary>
    public static ExamTemplateSectionResponse FromEntity(ExamTemplateSection e) =>
        new(e.Id, e.ExamTemplateId,
            e.TopicId, e.Topic?.Name,
            e.QuestionTypeId, e.QuestionType?.Name,
            e.CognitiveLevelId, e.CognitiveLevel?.Name,
            e.SectionName, e.QuestionCount, e.ScorePerQuestion,
            e.SortOrder, e.PctEasy, e.PctMedium, e.PctHard, e.PctVeryHard);
}

// ── ExamTemplate ──────────────────────────────────────────────────────────────

/// <summary>Request DTO để tạo / cập nhật mẫu đề thi</summary>
public record ExamTemplateRequest(
    int GradeLevelId,
    int SubjectId,
    string Title,
    string? Description,
    int DurationMinutes,
    int? TotalQuestions,
    decimal TotalScore,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    bool PreventDuplicate,
    string? Instructions,
    bool IsActive,
    IEnumerable<ExamTemplateSectionRequest> Sections
)
{
    /// <summary>Map sang entity để tạo mới</summary>
    public ExamTemplate ToEntity(string createdBy) => new()
    {
        GradeLevelId      = GradeLevelId,
        SubjectId         = SubjectId,
        CreatedBy         = createdBy,
        Title             = Title,
        Description       = Description,
        DurationMinutes   = DurationMinutes,
        TotalQuestions    = TotalQuestions,
        TotalScore        = TotalScore,
        ShuffleQuestions  = ShuffleQuestions,
        ShuffleAnswers    = ShuffleAnswers,
        PreventDuplicate  = PreventDuplicate,
        Instructions      = Instructions,
        IsActive          = IsActive
    };

    /// <summary>Map danh sách sections</summary>
    public IEnumerable<ExamTemplateSection> ToSections() =>
        Sections.Select(s => s.ToEntity());
}

/// <summary>Response DTO cho mẫu đề thi</summary>
public record ExamTemplateResponse(
    Guid Id,
    int GradeLevelId,
    string? GradeLevelName,
    int SubjectId,
    string? SubjectName,
    string Title,
    string? Description,
    int DurationMinutes,
    int? TotalQuestions,
    decimal TotalScore,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    bool PreventDuplicate,
    string? Instructions,
    bool IsActive,
    string? CreatedBy,
    long Created,
    long Modified,
    IReadOnlyList<ExamTemplateSectionResponse>? Sections
)
{
    /// <summary>Map từ entity</summary>
    public static ExamTemplateResponse FromEntity(ExamTemplate e, bool includeSections = false) =>
        new(
            e.Id,
            e.GradeLevelId, e.GradeLevel?.Name,
            e.SubjectId,    e.Subject?.Name,
            e.Title, e.Description,
            e.DurationMinutes, e.TotalQuestions,
            e.TotalScore, e.ShuffleQuestions, e.ShuffleAnswers,
            e.PreventDuplicate, e.Instructions,
            e.IsActive,
            e.CreatedBy,
            e.Created.ToTimestamp(),
            e.Modified.ToTimestamp(),
            includeSections ? e.Sections.Select(ExamTemplateSectionResponse.FromEntity).ToList() : null
        );
}

/// <summary>Thống kê mẫu đề thi (stat card).</summary>
public record ExamTemplateStatsResponse(int TotalTemplates, int ActiveTemplates, int TotalExamsGenerated, int AvgQuestions);
