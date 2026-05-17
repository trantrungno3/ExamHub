namespace ExamHub.Core.Domain.Enums;

/// <summary>
/// Trạng thái bài nộp của học sinh
/// </summary>
public enum SubmissionStatusEnum : byte
{
    /// <summary>Đang làm bài</summary>
    InProgress = 1,

    /// <summary>Đã nộp</summary>
    Submitted = 2,

    /// <summary>Đã chấm điểm</summary>
    Graded = 3
}

