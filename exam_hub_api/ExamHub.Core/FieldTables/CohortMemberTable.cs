namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng cohort_members (học sinh thuộc khoá học)
/// </summary>
public readonly struct CohortMemberTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.cohort_members";

    /// <summary>Khóa ngoại khoá học</summary>
    public const string CohortId = "cohort_id";

    /// <summary>Khóa ngoại học sinh (app_users)</summary>
    public const string StudentId = "student_id";

    /// <summary>Ban/lớp của học sinh: A, B, ...; NULL = chưa xếp lớp</summary>
    public const string Section = "section";

    /// <summary>Ngày tham gia khoá</summary>
    public const string JoinedAt = "joined_at";

    /// <summary>Đang học hay đã nghỉ</summary>
    public const string IsActive = "is_active";
}
