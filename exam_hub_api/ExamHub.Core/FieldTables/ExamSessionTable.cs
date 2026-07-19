namespace ExamHub.Core.FieldTables;

/// <summary>Tên bảng và cột cho bảng exam_sessions.</summary>
public readonly struct ExamSessionTable
{
    public const string TableName = "public.exam_sessions";
    public const string Title = "title";
    public const string Description = "description";
    public const string SubjectId = "subject_id";
    public const string GradeLevelId = "grade_level_id";
    public const string OpenAt = "open_at";
    public const string CloseAt = "close_at";
    public const string MaxAttempts = "max_attempts";
    public const string PickMode = "pick_mode";
    public const string Status = "status";
}
