namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng exams
/// </summary>
public readonly struct ExamTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.exams";

    /// <summary>Khóa ngoại mẫu đề thi</summary>
    public const string ExamTemplateId = "exam_template_id";

    /// <summary>Khóa ngoại lớp học</summary>
    public const string GradeLevelId = "grade_level_id";

    /// <summary>Khóa ngoại môn học</summary>
    public const string SubjectId = "subject_id";

    /// <summary>Người tạo</summary>
    public const string CreatedBy = "created_by";

    /// <summary>Tiêu đề đề thi</summary>
    public const string Title = "title";

    /// <summary>Mã đề thi (DE_001)</summary>
    public const string ExamCode = "exam_code";

    /// <summary>Thời gian làm bài (phút)</summary>
    public const string DurationMinutes = "duration_minutes";

    /// <summary>Tổng điểm</summary>
    public const string TotalScore = "total_score";

    /// <summary>Hướng dẫn làm bài</summary>
    public const string Instructions = "instructions";

    /// <summary>Trạng thái đề thi (draft, published, archived)</summary>
    public const string Status = "status";

    /// <summary>Năm học (2024-2025)</summary>
    public const string SchoolYear = "school_year";

    /// <summary>Học kỳ (1 hoặc 2)</summary>
    public const string Semester = "semester";

    /// <summary>Ngày thi</summary>
    public const string ExamDate = "exam_date";

    /// <summary>Tên lớp</summary>
    public const string ClassName = "class_name";

    /// <summary>Đề thi cha (sinh đề theo lô)</summary>
    public const string ParentExamId = "parent_exam_id";

    /// <summary>Chỉ số biến thể trong lô</summary>
    public const string VariantIndex = "variant_index";

    /// <summary>ID lô sinh đề</summary>
    public const string BatchId = "batch_id";

    /// <summary>Ngày tạo</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Ngày cập nhật</summary>
    public const string UpdatedAt = "updated_at";
}

