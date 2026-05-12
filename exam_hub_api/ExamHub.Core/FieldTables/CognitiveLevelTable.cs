namespace ExamHub.Core.FieldTables;

/// <summary>
/// Tên bảng và cột cho bảng cognitive_levels (Bloom's Taxonomy)
/// </summary>
public readonly struct CognitiveLevelTable
{
    /// <summary>Tên bảng</summary>
    public const string TableName = "public.cognitive_levels";

    /// <summary>Mã cấp độ: remember, understand, apply, analyze, evaluate, create</summary>
    public const string Code = "code";

    /// <summary>Tên tiếng Việt: Nhớ, Hiểu, Vận dụng, ...</summary>
    public const string Name = "name";

    /// <summary>Tên tiếng Anh: Remember, Understand, Apply, ...</summary>
    public const string NameEn = "name_en";

    /// <summary>Thứ tự từ thấp → cao (1–6)</summary>
    public const string LevelOrder = "level_order";

    /// <summary>Mô tả chi tiết cấp độ</summary>
    public const string Description = "description";

    /// <summary>Màu hex cho badge UI (#4CAF50, ...)</summary>
    public const string ColorCode = "color_code";

    /// <summary>Kích hoạt</summary>
    public const string IsActive = "is_active";
}
