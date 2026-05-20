using System.ComponentModel.DataAnnotations;

namespace ExamHub.Core.Application.Services;

/// <summary>
/// Interface cho dịch vụ sinh đề thi tự động theo cấu hình.
/// </summary>
public interface IExamGeneratorService
{
    /// <summary>Sinh đề thi từ request cấu hình và lưu vào database.</summary>
    Task<Guid> GenerateAsync(GenerateExamRequest request, CancellationToken ct = default);

    /// <summary>Sinh lô biến thể đề thi (cùng câu hỏi, xáo thứ tự khác nhau) trong một transaction.</summary>
    Task<BatchGenerateResult> BatchGenerateAsync(BatchGenerateExamRequest request, CancellationToken ct = default);
}

/// <summary>Kết quả sinh lô đề — trả về batchId và danh sách variant.</summary>
public sealed record BatchGenerateResult(
    Guid BatchId,
    IReadOnlyList<VariantInfo> Variants);

/// <summary>Thông tin một variant trong lô.</summary>
public sealed record VariantInfo(
    Guid ExamId,
    string? ExamCode,
    int VariantIndex,
    string VariantCode);

/// <summary>Cấu hình sinh đề thi.</summary>
public sealed record GenerateExamRequest(
    string Title,
    Guid? ExamTemplateId,
    int GradeLevelId,
    int SubjectId,
    int DurationMinutes,
    bool ShuffleQuestions,
    Guid CreatedBy,
    IReadOnlyList<SectionConfig> Sections);

/// <summary>Cấu hình một phần thi.</summary>
public sealed record SectionConfig(
    string? SectionName,
    [property: Range(1, int.MaxValue, ErrorMessage = "Chủ đề phần thi không hợp lệ.")]
    int TopicId,
    int? QuestionTypeId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Cấp độ nhận thức không hợp lệ.")]
    int? CognitiveLevelId,
    [property: Range(1, 200, ErrorMessage = "Số câu hỏi mỗi phần phải từ 1 đến 200.")]
    int QuestionCount,
    decimal PctEasy,
    decimal PctMedium,
    decimal PctHard,
    decimal PctVeryHard,
    [property: Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Điểm mỗi câu phải lớn hơn 0.")]
    decimal ScorePerQuestion) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PctEasy + PctMedium + PctHard + PctVeryHard != 100)
            yield return new ValidationResult(
                "Tổng tỉ lệ độ khó phải bằng 100%.",
                [nameof(PctEasy), nameof(PctMedium), nameof(PctHard), nameof(PctVeryHard)]);
    }
}

/// <summary>Request sinh lô đề thi nhiều biến thể.</summary>
public sealed record BatchGenerateExamRequest(
    string Title,
    Guid? ExamTemplateId,
    int GradeLevelId,
    int SubjectId,
    int DurationMinutes,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    int VariantCount,
    string VariantNaming,
    Guid CreatedBy,
    IReadOnlyList<SectionConfig> Sections);
