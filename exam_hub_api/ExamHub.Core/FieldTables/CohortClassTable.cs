namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng cohort_classes (lớp học sinh tự động từ khoá)
/// </summary>
public readonly struct CohortClassTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.cohort_classes";

    /// <summary>Khóa ngoại khoá học</summary>
    public const string CohortId = "cohort_id";

    /// <summary>Khóa ngoại khối lớp</summary>
    public const string GradeLevelId = "grade_level_id";

    /// <summary>Tên lớp: 1A, 10A, ...</summary>
    public const string ClassName = "class_name";

    /// <summary>Ban/lớp: A, B, C, ...</summary>
    public const string Section = "section";

    /// <summary>Năm học: 2020-2021, ...</summary>
    public const string SchoolYear = "school_year";

    /// <summary>Năm thứ mấy của khoá (1, 2, 3, ...)</summary>
    public const string YearIndex = "year_index";

    /// <summary>ID giáo viên chủ nhiệm</summary>
    public const string HomeroomTeacherId = "homeroom_teacher_id";
}
