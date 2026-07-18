using System.ComponentModel.DataAnnotations;
using ExamHub.Core.Application.Services;

namespace ExamHub.Core.DataTransferObjects.Exam;

/// <summary>Request DTO để sinh đề thi qua API — CreatedBy được lấy từ JWT.</summary>
public sealed record GenerateExamApiRequest(
    [property: Required(ErrorMessage = "Tiêu đề đề thi không được để trống.")]
    [property: MaxLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự.")]
    string Title,
    Guid? ExamTemplateId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Lớp học không hợp lệ.")]
    int GradeLevelId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Môn học không hợp lệ.")]
    int SubjectId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Thời gian làm bài phải lớn hơn 0 phút.")]
    int DurationMinutes,
    bool ShuffleQuestions,
    [property: Required(ErrorMessage = "Đề thi phải có ít nhất một phần thi.")]
    [property: MinLength(1, ErrorMessage = "Đề thi phải có ít nhất một phần thi.")]
    IReadOnlyList<SectionConfig> Sections)
{
    /// <summary>Chuyển đổi sang service request với CreatedBy từ JWT.</summary>
    public GenerateExamRequest ToServiceRequest(string createdBy) => new(
        Title, ExamTemplateId, GradeLevelId, SubjectId,
        DurationMinutes, ShuffleQuestions, createdBy, Sections);
}
