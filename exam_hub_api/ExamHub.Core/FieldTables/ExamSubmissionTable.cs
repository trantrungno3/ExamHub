namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng exam_submissions
/// </summary>
public readonly struct ExamSubmissionTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.exam_submissions";

    /// <summary>Khóa ngoại đề thi</summary>
    public const string ExamId = "exam_id";

    /// <summary>Khóa ngoại học sinh</summary>
    public const string StudentId = "student_id";

    /// <summary>Thời điểm bắt đầu làm bài</summary>
    public const string StartedAt = "started_at";

    /// <summary>Thời điểm nộp bài</summary>
    public const string SubmittedAt = "submitted_at";

    /// <summary>Thời gian làm bài (giây)</summary>
    public const string DurationSeconds = "duration_seconds";

    /// <summary>Tổng điểm đạt được</summary>
    public const string TotalScore = "total_score";

    /// <summary>Đã vượt qua hay chưa</summary>
    public const string IsPassed = "is_passed";

    /// <summary>Trạng thái bài nộp</summary>
    public const string Status = "status";
}

