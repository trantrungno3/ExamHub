using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Mức độ khó của câu hỏi
/// </summary>
[Table(DifficultyLevelTable.TableName)]
[SqlBuilderProperty(DifficultyLevelTable.TableName)]
public class DifficultyLevel : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty("id", Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Mã mức độ khó, VD: "easy", "medium", "hard", "very_hard"</summary>
    [Column(DifficultyLevelTable.Code)]
    [SqlBuilderProperty(DifficultyLevelTable.Code, Insert = true, Update = true)]
    public required string Code { get; set; }

    /// <summary>Tên mức độ khó, VD: "Dễ", "Trung bình"</summary>
    [Column(DifficultyLevelTable.Name)]
    [SqlBuilderProperty(DifficultyLevelTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Hệ số điểm (1.0, 1.5, 2.0, 2.5)</summary>
    [Column(DifficultyLevelTable.ScoreWeight)]
    [SqlBuilderProperty(DifficultyLevelTable.ScoreWeight, Insert = true, Update = true)]
    public decimal ScoreWeight { get; set; } = 1.0m;

    /// <summary>Thứ tự sắp xếp</summary>
    [Column(DifficultyLevelTable.SortOrder)]
    [SqlBuilderProperty(DifficultyLevelTable.SortOrder, Insert = true, Update = true)]
    public short SortOrder { get; set; } = 0;

    /// <summary>Kích hoạt hay không</summary>
    [Column(DifficultyLevelTable.IsActive)]
    [SqlBuilderProperty(DifficultyLevelTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        code         = Code,
        name         = Name,
        score_weight = ScoreWeight,
        sort_order   = SortOrder,
        is_active    = IsActive
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id           = Id,
        name         = Name,
        score_weight = ScoreWeight,
        sort_order   = SortOrder,
        is_active    = IsActive
    };
}
