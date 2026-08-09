namespace ExamHub.Core.DataTransferObjects.Question;

/// <summary>Thống kê số lượng câu hỏi theo trạng thái (cho stat card ngân hàng câu hỏi).</summary>
public record QuestionStatsResponse(int Total, int Verified, int Unverified, int Inactive);
