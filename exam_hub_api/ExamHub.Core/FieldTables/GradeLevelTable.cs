namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng grade_levels
/// </summary>
public readonly struct GradeLevelTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.grade_levels";

    /// <summary>Tên lớp (VD: "Lớp 10")</summary>
    public const string Name = "name";

    /// <summary>Số lớp (1 → 12)</summary>
    public const string GradeNumber = "grade_number";

    /// <summary>Mô tả</summary>
    public const string Description = "description";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";
}

