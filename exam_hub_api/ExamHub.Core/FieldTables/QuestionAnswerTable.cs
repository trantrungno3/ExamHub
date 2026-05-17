namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng question_answers
/// </summary>
public readonly struct QuestionAnswerTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.question_answers";

    /// <summary>Khóa ngoại câu hỏi</summary>
    public const string QuestionId = "question_id";

    /// <summary>Nội dung đáp án</summary>
    public const string Content = "content";

    /// <summary>Nội dung thuần text</summary>
    public const string ContentPlain = "content_plain";

    /// <summary>Đây là đáp án đúng</summary>
    public const string IsCorrect = "is_correct";

    /// <summary>Thứ tự hiển thị</summary>
    public const string SortOrder = "sort_order";

    /// <summary>Giải thích cho đáp án này</summary>
    public const string Explanation = "explanation";
}

