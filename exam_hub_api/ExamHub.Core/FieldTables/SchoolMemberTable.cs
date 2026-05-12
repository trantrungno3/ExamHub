namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng school_members (giáo viên/admin thuộc trường)
/// </summary>
public readonly struct SchoolMemberTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.school_members";

    /// <summary>Khóa ngoại trường học</summary>
    public const string SchoolId = "school_id";

    /// <summary>Khóa ngoại người dùng (app_users)</summary>
    public const string UserId = "user_id";

    /// <summary>Vai trò ngữ cảnh trong trường: Admin, Teacher</summary>
    public const string Role = "role";

    /// <summary>Đang hoạt động</summary>
    public const string IsActive = "is_active";

    /// <summary>Ngày tham gia trường</summary>
    public const string JoinedAt = "joined_at";
}