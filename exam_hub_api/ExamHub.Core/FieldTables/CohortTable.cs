namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng cohorts (khoá học tuyển sinh)
/// </summary>
public readonly struct CohortTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.cohorts";

    /// <summary>Khóa ngoại trường học</summary>
    public const string SchoolId = "school_id";

    /// <summary>Tên khoá: Khoá 2020-2025</summary>
    public const string Name = "name";

    /// <summary>Năm bắt đầu: 2020</summary>
    public const string StartYear = "start_year";

    /// <summary>Năm kết thúc: 2025</summary>
    public const string EndYear = "end_year";

    /// <summary>Lớp bắt đầu: 1, 6, 10</summary>
    public const string GradeStart = "grade_start";

    /// <summary>Số lớp song song: 1 → A, 2 → A,B, ...</summary>
    public const string NumClasses = "num_classes";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";
}
