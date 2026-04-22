namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng exam_templates
/// </summary>
public readonly struct ExamTemplateTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.exam_templates";

    /// <summary>Khóa ngoại lớp học</summary>
    public const string GradeLevelId = "grade_level_id";

    /// <summary>Khóa ngoại môn học</summary>
    public const string SubjectId = "subject_id";

    /// <summary>Người tạo</summary>
    public const string CreatedBy = "created_by";

    /// <summary>Tiêu đề mẫu đề thi</summary>
    public const string Title = "title";

    /// <summary>Mô tả</summary>
    public const string Description = "description";

    /// <summary>Thời gian làm bài (phút)</summary>
    public const string DurationMinutes = "duration_minutes";

    /// <summary>Tổng số câu hỏi</summary>
    public const string TotalQuestions = "total_questions";

    /// <summary>Tổng điểm</summary>
    public const string TotalScore = "total_score";

    /// <summary>Xáo trộn câu hỏi</summary>
    public const string ShuffleQuestions = "shuffle_questions";

    /// <summary>Xáo trộn đáp án</summary>
    public const string ShuffleAnswers = "shuffle_answers";

    /// <summary>Tránh trùng câu hỏi</summary>
    public const string PreventDuplicate = "prevent_duplicate";

    /// <summary>Hướng dẫn làm bài</summary>
    public const string Instructions = "instructions";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";

    /// <summary>Ngày tạo</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Ngày cập nhật</summary>
    public const string UpdatedAt = "updated_at";
}

