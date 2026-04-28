using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Đại diện cho một lớp học trong hệ thống (Lớp 1 – Lớp 12)
/// </summary>
[Table(GradeLevelTable.TableName)]
[SqlBuilderProperty(GradeLevelTable.TableName)]
public class GradeLevel : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL - tự động tăng)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Tên lớp học, VD: "Lớp 10"</summary>
    [Column(GradeLevelTable.Name)]
    [SqlBuilderProperty(GradeLevelTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Số lớp (1 → 12)</summary>
    [Column(GradeLevelTable.GradeNumber)]
    [SqlBuilderProperty(GradeLevelTable.GradeNumber, Insert = true, Update = true)]
    public short GradeNumber { get; set; }

    /// <summary>Mô tả thêm về lớp</summary>
    [Column(GradeLevelTable.Description)]
    [SqlBuilderProperty(GradeLevelTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    /// <summary>Kích hoạt hay không</summary>
    [Column(GradeLevelTable.IsActive)]
    [SqlBuilderProperty(GradeLevelTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    /// <summary>Thời điểm tạo</summary>
    [Column(GradeLevelTable.CreatedAt)]
    [SqlBuilderProperty(GradeLevelTable.CreatedAt, Insert = true, Update = false)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời điểm cập nhật gần nhất</summary>
    [Column(GradeLevelTable.UpdatedAt)]
    [SqlBuilderProperty(GradeLevelTable.UpdatedAt, Insert = true, Update = true)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Danh sách môn học thuộc lớp này</summary>
    public List<Subject> Subjects { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        name        = Name,
        grade_number = GradeNumber,
        description = Description,
        is_active   = IsActive,
        created_at  = CreatedAt,
        updated_at  = UpdatedAt
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id          = Id,
        name        = Name,
        grade_number = GradeNumber,
        description = Description,
        is_active   = IsActive,
        updated_at  = DateTime.UtcNow
    };
}
