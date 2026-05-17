namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng topics
/// </summary>
public readonly struct TopicTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.topics";

    /// <summary>Khóa ngoại môn học</summary>
    public const string SubjectId = "subject_id";

    /// <summary>Khóa ngoại chủ đề cha</summary>
    public const string ParentId = "parent_id";

    /// <summary>Tên chủ đề</summary>
    public const string Name = "name";

    /// <summary>Mã chủ đề</summary>
    public const string Code = "code";

    /// <summary>Thứ tự hiển thị</summary>
    public const string SortOrder = "sort_order";

    /// <summary>Mô tả</summary>
    public const string Description = "description";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";

    /// <summary>Ngày tạo</summary>
    public const string CreatedAt = "created_at";

    /// <summary>Ngày cập nhật</summary>
    public const string UpdatedAt = "updated_at";
}

