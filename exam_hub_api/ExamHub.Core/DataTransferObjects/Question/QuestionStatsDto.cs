namespace ExamHub.Core.DataTransferObjects.Question;

/// <summary>Thống kê số lượng câu hỏi theo trạng thái (cho stat card ngân hàng câu hỏi).</summary>
public record QuestionStatsResponse(int Total, int Verified, int Pending, int Rejected, int Inactive);

/// <summary>Request từ chối câu hỏi kèm lý do.</summary>
public record RejectQuestionRequest(string Reason);
