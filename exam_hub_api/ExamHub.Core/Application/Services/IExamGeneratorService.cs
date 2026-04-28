namespace ExamHub.Core.Application.Services;

/// <summary>
/// Interface cho dịch vụ sinh đề thi tự động theo cấu hình.
/// </summary>
public interface IExamGeneratorService
{
    /// <summary>Sinh đề thi từ request cấu hình và lưu vào database.</summary>
    Task<Guid> GenerateAsync(GenerateExamRequest request, CancellationToken ct = default);
}

/// <summary>Cấu hình sinh đề thi.</summary>
public sealed record GenerateExamRequest(
    string Title,
    Guid? ExamTemplateId,
    int GradeLevelId,
    int SubjectId,
    bool ShuffleQuestions,
    IReadOnlyList<SectionConfig> Sections);

/// <summary>Cấu hình một phần thi.</summary>
public sealed record SectionConfig(
    string? SectionName,
    int TopicId,
    int? QuestionTypeId,
    int QuestionCount,
    decimal PctEasy,
    decimal PctMedium,
    decimal PctHard,
    decimal PctVeryHard,
    decimal ScorePerQuestion);

