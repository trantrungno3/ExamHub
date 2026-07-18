using System.ComponentModel.DataAnnotations;
using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.Question;

// ── Answer ──────────────────────────────────────────────────────────────────

/// <summary>Request DTO cho một đáp án</summary>
public record QuestionAnswerRequest(
    string Content,
    string? ContentPlain,
    bool IsCorrect,
    string? Explanation
);

/// <summary>Response DTO cho một đáp án</summary>
public record QuestionAnswerResponse(
    Guid Id,
    string Content,
    string? ContentPlain,
    bool IsCorrect,
    short SortOrder,
    string? Explanation
)
{
    /// <summary>Map từ entity</summary>
    public static QuestionAnswerResponse FromEntity(QuestionAnswer e) =>
        new(e.Id, e.Content, e.ContentPlain, e.IsCorrect, e.SortOrder, e.Explanation);
}

// ── Question ─────────────────────────────────────────────────────────────────

/// <summary>Request DTO để tạo / cập nhật câu hỏi</summary>
public record QuestionRequest(
    [property: Range(1, int.MaxValue, ErrorMessage = "Chủ đề không hợp lệ.")]
    int TopicId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Loại câu hỏi không hợp lệ.")]
    int QuestionTypeId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Mức độ khó không hợp lệ.")]
    int DifficultyLevelId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Cấp độ nhận thức không hợp lệ.")]
    int? CognitiveLevelId,
    [property: Required(ErrorMessage = "Nội dung câu hỏi không được để trống.")]
    string Content,
    string? ContentPlain,
    string? Explanation,
    string? ImageUrl,
    string? AudioUrl,
    string? Source,
    string[]? Tags,
    [property: Required(ErrorMessage = "Câu hỏi phải có ít nhất một đáp án.")]
    [property: HasCorrectAnswer]
    IEnumerable<QuestionAnswerRequest> Answers
)
{
    /// <summary>Map sang entity Question</summary>
    public Domain.Entities.Question ToEntity(string createdBy) => new()
    {
        TopicId           = TopicId,
        QuestionTypeId    = QuestionTypeId,
        DifficultyLevelId = DifficultyLevelId,
        CognitiveLevelId  = CognitiveLevelId,
        CreatedBy         = createdBy,
        Content           = Content,
        ContentPlain      = ContentPlain,
        Explanation       = Explanation,
        ImageUrl          = ImageUrl,
        AudioUrl          = AudioUrl,
        Source            = Source,
        Tags              = Tags ?? []
    };

    /// <summary>Map sang danh sách QuestionAnswer</summary>
    public IEnumerable<QuestionAnswer> ToAnswers() =>
        Answers.Select(a => new QuestionAnswer
        {
            Content      = a.Content,
            ContentPlain = a.ContentPlain,
            IsCorrect    = a.IsCorrect,
            Explanation  = a.Explanation
        });
}

/// <summary>Response DTO cho câu hỏi</summary>
public record QuestionResponse(
    Guid Id,
    int TopicId,
    string? TopicName,
    int QuestionTypeId,
    string? QuestionTypeName,
    int DifficultyLevelId,
    string? DifficultyLevelName,
    int? CognitiveLevelId,
    string? CognitiveLevelName,
    string Content,
    string? ContentPlain,
    string? Explanation,
    string? ImageUrl,
    string? AudioUrl,
    string? Source,
    string[] Tags,
    int UsageCount,
    bool IsActive,
    bool IsVerified,
    long Created,
    long Modified,
    IReadOnlyList<QuestionAnswerResponse>? Answers
)
{
    /// <summary>Map từ entity</summary>
    public static QuestionResponse FromEntity(Domain.Entities.Question e, bool includeAnswers = false) =>
        new(
            e.Id,
            e.TopicId,
            e.Topic?.Name,
            e.QuestionTypeId,
            e.QuestionType?.Name,
            e.DifficultyLevelId,
            e.DifficultyLevel?.Name,
            e.CognitiveLevelId,
            e.CognitiveLevel?.Name,
            e.Content,
            e.ContentPlain,
            e.Explanation,
            e.ImageUrl,
            e.AudioUrl,
            e.Source,
            e.Tags,
            e.UsageCount,
            e.IsActive,
            e.IsVerified,
            e.Created.ToTimestamp(),
            e.Modified.ToTimestamp(),
            includeAnswers ? e.Answers.Select(QuestionAnswerResponse.FromEntity).ToList() : null
        );
}

/// <summary>Request lọc danh sách câu hỏi phân trang</summary>
public record QuestionPagedRequest(
    int Page = 1,
    int PageSize = 20,
    int? TopicId = null,
    int? QuestionTypeId = null,
    int? DifficultyLevelId = null,
    int? CognitiveLevelId = null,
    string? Keyword = null,
    bool? IsVerified = null
);

// ── Custom Validation Attribute ───────────────────────────────────────────────

[AttributeUsage(AttributeTargets.Property)]
public sealed class HasCorrectAnswerAttribute : ValidationAttribute
{
    public HasCorrectAnswerAttribute() : base("Câu hỏi phải có ít nhất một đáp án đúng.") { }

    public override bool IsValid(object? value) =>
        value is not IEnumerable<QuestionAnswerRequest> answers || answers.Any(a => a.IsCorrect);
}
