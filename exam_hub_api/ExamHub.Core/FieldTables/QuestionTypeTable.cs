namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng question_types
/// </summary>
public readonly struct QuestionTypeTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.question_types";

    /// <summary>Mã loại câu hỏi (multiple_choice, true_false, ...)</summary>
    public const string Code = "code";

    /// <summary>Tên loại câu hỏi</summary>
    public const string Name = "name";

    /// <summary>Mô tả</summary>
    public const string Description = "description";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";
}

