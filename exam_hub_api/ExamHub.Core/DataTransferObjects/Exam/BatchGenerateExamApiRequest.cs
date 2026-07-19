using System.ComponentModel.DataAnnotations;
using ExamHub.Core.Application.Services;

namespace ExamHub.Core.DataTransferObjects.Exam;

/// <summary>Request sinh lô đề thi (nhiều biến thể cùng lúc).</summary>
public sealed record BatchGenerateExamApiRequest
{
    [Required(ErrorMessage = "Tiêu đề đề thi không được để trống.")]
    [MaxLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự.")]
    public string Title { get; set; }

    public Guid? ExamTemplateId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lớp học không hợp lệ.")]
    public int GradeLevelId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Môn học không hợp lệ.")]
    public int SubjectId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Thời gian làm bài phải lớn hơn 0 phút.")]
    public int DurationMinutes { get; set; }

    public bool ShuffleQuestions { get; set; }
    public bool ShuffleAnswers { get; set; }
    public bool PreventDuplicate { get; set; } = true;

    /// <summary>Tổng điểm của đề (tuỳ chọn) — bỏ trống/0 để tự tính từ các phần.</summary>
    [Range(0, 9999999, ErrorMessage = "Tổng điểm không hợp lệ.")]
    public decimal TotalScore { get; set; }

    [Range(1, 20, ErrorMessage = "Số biến thể phải từ 1 đến 20.")]
    public int VariantCount { get; set; }

    [Required(ErrorMessage = "VariantNaming không được để trống.")]
    [RegularExpression("^(ALPHA|NUMBER)$", ErrorMessage = "VariantNaming phải là 'ALPHA' hoặc 'NUMBER'.")]
    public string VariantNaming { get; set; }

    [Required(ErrorMessage = "Đề thi phải có ít nhất một phần thi.")]
    [MinLength(1, ErrorMessage = "Đề thi phải có ít nhất một phần thi.")]
    public IReadOnlyList<SectionConfig> Sections { get; set; }

    /// <summary>Chuyển đổi sang service request với CreatedBy từ JWT.</summary>
    public BatchGenerateExamRequest ToServiceRequest(string createdBy) => new(
        Title, ExamTemplateId, GradeLevelId, SubjectId,
        DurationMinutes, ShuffleQuestions, ShuffleAnswers, PreventDuplicate, TotalScore,
        VariantCount, VariantNaming, createdBy, Sections);
}

/// <summary>Kết quả sinh lô đề thi.</summary>
public sealed record BatchGenerateExamResponse(
    Guid BatchId,
    IReadOnlyList<VariantSummaryResponse> Variants);

/// <summary>Tóm tắt một biến thể đề thi trong lô.</summary>
public sealed record VariantSummaryResponse(
    Guid ExamId,
    string? ExamCode,
    int VariantIndex,
    string VariantCode);