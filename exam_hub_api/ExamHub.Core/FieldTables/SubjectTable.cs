namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng subjects
/// </summary>
public readonly struct SubjectTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.subjects";

    /// <summary>Khóa ngoại lớp học</summary>
    public const string GradeLevelId = "grade_level_id";

    /// <summary>Tên môn học</summary>
    public const string Name = "name";

    /// <summary>Mã môn học (VD: MATH, LIT)</summary>
    public const string Code = "code";

    /// <summary>Mô tả</summary>
    public const string Description = "description";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";

    /// <summary>Ngày tạo</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Ngày cập nhật</summary>
    public const string UpdatedAt = "updated_at";
}

