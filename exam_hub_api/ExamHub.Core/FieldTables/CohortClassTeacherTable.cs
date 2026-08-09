namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng cohort_class_teachers (phân công GV giảng dạy cho lớp)
/// </summary>
public readonly struct CohortClassTeacherTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.cohort_class_teachers";

    /// <summary>Khóa ngoại lớp học</summary>
    public const string CohortClassId = "cohort_class_id";

    /// <summary>Khóa ngoại môn học</summary>
    public const string SubjectId = "subject_id";

    /// <summary>Khóa ngoại người dùng (giáo viên)</summary>
    public const string TeacherId = "teacher_id";
}
