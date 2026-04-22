namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng submission_answers
/// </summary>
public readonly struct SubmissionAnswerTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.submission_answers";

    /// <summary>Khóa ngoại bài nộp</summary>
    public const string SubmissionId = "submission_id";

    /// <summary>Khóa ngoại câu hỏi trong đề thi</summary>
    public const string ExamQuestionId = "exam_question_id";

    /// <summary>Danh sách ID đáp án đã chọn (UUID[])</summary>
    public const string SelectedAnswerIds = "selected_answer_ids";

    /// <summary>Nội dung câu tự luận</summary>
    public const string EssayContent = "essay_content";

    /// <summary>Câu trả lời đúng/sai</summary>
    public const string IsCorrect = "is_correct";

    /// <summary>Điểm đạt được</summary>
    public const string ScoreEarned = "score_earned";

    /// <summary>Nhận xét/phản hồi</summary>
    public const string Feedback = "feedback";

    /// <summary>Người chấm điểm</summary>
    public const string GradedBy = "graded_by";
}

