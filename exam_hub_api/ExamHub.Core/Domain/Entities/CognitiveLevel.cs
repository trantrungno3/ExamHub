using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Cấp độ nhận thức theo Bloom's Taxonomy (2001 revision)
/// </summary>
[Table(CognitiveLevelTable.TableName)]
[SqlBuilderProperty(CognitiveLevelTable.TableName)]
public class CognitiveLevel : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty("id", Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Mã cấp độ: remember, understand, apply, analyze, evaluate, create</summary>
    [Column(CognitiveLevelTable.Code)]
    [SqlBuilderProperty(CognitiveLevelTable.Code, Insert = true, Update = true)]
    public required string Code { get; set; }

    /// <summary>Tên tiếng Việt: Nhớ, Hiểu, Vận dụng, Phân tích, Đánh giá, Sáng tạo</summary>
    [Column(CognitiveLevelTable.Name)]
    [SqlBuilderProperty(CognitiveLevelTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Tên tiếng Anh: Remember, Understand, Apply, Analyze, Evaluate, Create</summary>
    [Column(CognitiveLevelTable.NameEn)]
    [SqlBuilderProperty(CognitiveLevelTable.NameEn, Insert = true, Update = true)]
    public required string NameEn { get; set; }

    /// <summary>Thứ tự từ thấp → cao (1–6)</summary>
    [Column(CognitiveLevelTable.LevelOrder)]
    [SqlBuilderProperty(CognitiveLevelTable.LevelOrder, Insert = true, Update = true)]
    public short LevelOrder { get; set; }

    /// <summary>Mô tả chi tiết, các động từ hành động tiêu biểu</summary>
    [Column(CognitiveLevelTable.Description)]
    [SqlBuilderProperty(CognitiveLevelTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    /// <summary>Màu hex cho badge UI (#4CAF50, #2196F3, ...)</summary>
    [Column(CognitiveLevelTable.ColorCode)]
    [SqlBuilderProperty(CognitiveLevelTable.ColorCode, Insert = true, Update = true)]
    public string? ColorCode { get; set; }

    /// <summary>Kích hoạt hay không</summary>
    [Column(CognitiveLevelTable.IsActive)]
    [SqlBuilderProperty(CognitiveLevelTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        code        = Code,
        name        = Name,
        name_en     = NameEn,
        level_order = LevelOrder,
        description = Description,
        color_code  = ColorCode,
        is_active   = IsActive
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id          = Id,
        name        = Name,
        name_en     = NameEn,
        level_order = LevelOrder,
        description = Description,
        color_code  = ColorCode,
        is_active   = IsActive
    };
}