using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Môn học gắn với một lớp học cụ thể
/// </summary>
[Table(SubjectTable.TableName)]
[SqlBuilderProperty(SubjectTable.TableName)]
public class Subject : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Khóa ngoại lớp học</summary>
    [Column(SubjectTable.GradeLevelId)]
    [SqlBuilderProperty(SubjectTable.GradeLevelId, Insert = true, Update = true)]
    public int GradeLevelId { get; set; }

    /// <summary>Tên môn học, VD: "Toán", "Ngữ văn"</summary>
    [Column(SubjectTable.Name)]
    [SqlBuilderProperty(SubjectTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Mã môn học, VD: "MATH", "LIT"</summary>
    [Column(SubjectTable.Code)]
    [SqlBuilderProperty(SubjectTable.Code, Insert = true, Update = true)]
    public required string Code { get; set; }

    /// <summary>Mô tả</summary>
    [Column(SubjectTable.Description)]
    [SqlBuilderProperty(SubjectTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    /// <summary>Kích hoạt hay không</summary>
    [Column(SubjectTable.IsActive)]
    [SqlBuilderProperty(SubjectTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    /// <summary>Thời điểm tạo</summary>
    [Column(SubjectTable.CreatedAt)]
    [SqlBuilderProperty(SubjectTable.CreatedAt, Insert = true, Update = false)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời điểm cập nhật gần nhất</summary>
    [Column(SubjectTable.UpdatedAt)]
    [SqlBuilderProperty(SubjectTable.UpdatedAt, Insert = true, Update = true)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Lớp học sở hữu môn này</summary>
    public GradeLevel? GradeLevel { get; set; }

    /// <summary>Danh sách chủ đề / chương của môn học</summary>
    public List<Topic> Topics { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        grade_level_id = GradeLevelId,
        name           = Name,
        code           = Code,
        description    = Description,
        is_active      = IsActive,
        created_at     = CreatedAt,
        updated_at     = UpdatedAt
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id             = Id,
        grade_level_id = GradeLevelId,
        name           = Name,
        code           = Code,
        description    = Description,
        is_active      = IsActive,
        updated_at     = DateTime.UtcNow
    };
}
