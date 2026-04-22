namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng difficulty_levels
/// </summary>
public readonly struct DifficultyLevelTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.difficulty_levels";

    /// <summary>Mã độ khó (easy, medium, hard, very_hard)</summary>
    public const string Code = "code";

    /// <summary>Tên độ khó</summary>
    public const string Name = "name";

    /// <summary>Hệ số điểm</summary>
    public const string ScoreWeight = "score_weight";

    /// <summary>Thứ tự sắp xếp</summary>
    public const string SortOrder = "sort_order";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";
}

