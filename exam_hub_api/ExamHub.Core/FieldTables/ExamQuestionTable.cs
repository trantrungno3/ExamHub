namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng exam_questions
/// </summary>
public readonly struct ExamQuestionTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.exam_questions";

    /// <summary>Khóa ngoại đề thi</summary>
    public const string ExamId = "exam_id";

    /// <summary>Khóa ngoại câu hỏi gốc</summary>
    public const string QuestionId = "question_id";

    /// <summary>Tên phần thi</summary>
    public const string SectionName = "section_name";

    /// <summary>Thứ tự trong đề</summary>
    public const string SortOrder = "sort_order";

    /// <summary>Điểm của câu hỏi này</summary>
    public const string Score = "score";

    /// <summary>Snapshot nội dung câu hỏi tại thời điểm tạo đề</summary>
    public const string ContentSnapshot = "content_snapshot";

    /// <summary>Snapshot đáp án (JSONB)</summary>
    public const string AnswersSnapshot = "answers_snapshot";
}

