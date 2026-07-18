using ExamHub.Core.DataTransferObjects.Question;

namespace ExamHub.Core.Application.Services;

/// <summary>
/// Dịch vụ import câu hỏi hàng loạt từ file Excel (.xlsx).
/// </summary>
/// <remarks>
/// Định dạng cột (dòng 1 là tiêu đề, dữ liệu từ dòng 2):
/// <list type="number">
/// <item>Content (bắt buộc)</item>
/// <item>QuestionTypeId (bắt buộc, số nguyên)</item>
/// <item>DifficultyLevelId (tuỳ chọn — trống thì dùng default)</item>
/// <item>TopicId (tuỳ chọn — trống thì dùng default)</item>
/// <item>CognitiveLevelId (tuỳ chọn — trống thì dùng default)</item>
/// <item>Explanation (tuỳ chọn)</item>
/// <item>AnswerA, AnswerB, AnswerC, AnswerD (tuỳ chọn)</item>
/// <item>CorrectAnswers — các chữ cái đáp án đúng, vd "A" hoặc "A,C"</item>
/// </list>
/// Mỗi dòng được xử lý độc lập; dòng lỗi được báo cáo, dòng hợp lệ vẫn được lưu (import một phần).
/// </remarks>
public interface IBulkImportService
{
    /// <summary>Phân tích file Excel và lưu các câu hỏi hợp lệ.</summary>
    Task<BulkImportQuestionResponse> ImportAsync(
        BulkImportQuestionRequest request, string createdBy, CancellationToken ct = default);
}
