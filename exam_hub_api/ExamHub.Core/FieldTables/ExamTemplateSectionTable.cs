namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng exam_template_sections
/// </summary>
public readonly struct ExamTemplateSectionTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.exam_template_sections";

    /// <summary>Khóa ngoại mẫu đề thi</summary>
    public const string ExamTemplateId = "exam_template_id";

    /// <summary>Khóa ngoại chủ đề (null = toàn bộ môn)</summary>
    public const string TopicId = "topic_id";

    /// <summary>Khóa ngoại loại câu hỏi (null = tất cả loại)</summary>
    public const string QuestionTypeId = "question_type_id";

    /// <summary>Khóa ngoại cấp độ nhận thức Bloom để lọc pool (nullable)</summary>
    public const string CognitiveLevelId = "cognitive_level_id";

    /// <summary>Tên phần thi</summary>
    public const string SectionName = "section_name";

    /// <summary>Số câu hỏi trong phần</summary>
    public const string QuestionCount = "question_count";

    /// <summary>Điểm mỗi câu</summary>
    public const string ScorePerQuestion = "score_per_question";

    /// <summary>Thứ tự sắp xếp</summary>
    public const string SortOrder = "sort_order";

    /// <summary>Tỉ lệ % câu dễ</summary>
    public const string PctEasy = "pct_easy";

    /// <summary>Tỉ lệ % câu trung bình</summary>
    public const string PctMedium = "pct_medium";

    /// <summary>Tỉ lệ % câu khó</summary>
    public const string PctHard = "pct_hard";

    /// <summary>Tỉ lệ % câu rất khó</summary>
    public const string PctVeryHard = "pct_very_hard";

    /// <summary>Ngày tạo</summary>
    public const string CreatedAt = "created_at";
}

