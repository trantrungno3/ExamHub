namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng teacher_subjects
/// </summary>
public readonly struct TeacherSubjectTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.teacher_subjects";

    /// <summary>Khóa ngoại người dùng (giáo viên)</summary>
    public const string UserId = "user_id";

    /// <summary>Khóa ngoại môn học</summary>
    public const string SubjectId = "subject_id";
}

