using System.ComponentModel.DataAnnotations;
using ExamHub.Core.Application.Services;

namespace ExamHub.Core.DataTransferObjects.Exam;

/// <summary>Request DTO để sinh đề thi qua API — CreatedBy được lấy từ JWT.</summary>
public sealed record GenerateExamApiRequest( )
{
    [property: Required(ErrorMessage = "Tiêu đề đề thi không được để trống.")]
    [property: MaxLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự.")]
   public string Title { get; set; }
    public  Guid? ExamTemplateId{ get; set; }
    [property: Range(1, int.MaxValue, ErrorMessage = "Lớp học không hợp lệ.")]
    public int GradeLevelId{ get; set; }
    [property: Range(1, int.MaxValue, ErrorMessage = "Môn học không hợp lệ.")]
    public int SubjectId{ get; set; }
    [property: Range(1, int.MaxValue, ErrorMessage = "Thời gian làm bài phải lớn hơn 0 phút.")]
    public int DurationMinutes{ get; set; }
    public  bool ShuffleQuestions{ get; set; }
    public  bool ShuffleAnswers{ get; set; }
    public  bool PreventDuplicate{ get; set; } = true;
    /// <summary>Tổng điểm của đề (tuỳ chọn) — bỏ trống/0 để tự tính từ các phần.</summary>
    [property: Range(0, 9999999, ErrorMessage = "Tổng điểm không hợp lệ.")]
    public decimal TotalScore{ get; set; }
    [property: Required(ErrorMessage = "Đề thi phải có ít nhất một phần thi.")]
    [property: MinLength(1, ErrorMessage = "Đề thi phải có ít nhất một phần thi.")]
    public IReadOnlyList<SectionConfig> Sections{ get; set; }
    /// <summary>Chuyển đổi sang service request với CreatedBy từ JWT.</summary>
    public GenerateExamRequest ToServiceRequest(string createdBy) => new(
        Title, ExamTemplateId, GradeLevelId, SubjectId,
        DurationMinutes, ShuffleQuestions, ShuffleAnswers, PreventDuplicate, TotalScore, createdBy, Sections);
}
