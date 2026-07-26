using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Học sinh thuộc khoá học.
/// 1 học sinh chỉ thuộc 1 khoá trong 1 trường.
/// </summary>
[Table(CohortMemberTable.TableName)]
[SqlBuilderProperty(CohortMemberTable.TableName)]
public class CohortMember : ModifyModelBase, IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại khoá học</summary>
    [Column(CohortMemberTable.CohortId)]
    [SqlBuilderProperty(CohortMemberTable.CohortId, Insert = true, Update = false)]
    public int CohortId { get; set; }

    /// <summary>ID học sinh (app_users)</summary>
    [Column(CohortMemberTable.StudentId)]
    [SqlBuilderProperty(CohortMemberTable.StudentId, Insert = true, Update = false)]
    public Guid StudentId { get; set; }

    /// <summary>Ban/lớp của học sinh trong khoá (A, B, ...); NULL = chưa xếp lớp</summary>
    [Column(CohortMemberTable.Section)]
    [SqlBuilderProperty(CohortMemberTable.Section, Insert = true, Update = true)]
    public string? Section { get; set; }

    /// <summary>Ngày tham gia khoá</summary>
    [Column(CohortMemberTable.JoinedAt)]
    [SqlBuilderProperty(CohortMemberTable.JoinedAt, Insert = true, Update = false)]
    public DateOnly JoinedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Đang học hay đã nghỉ</summary>
    [Column(CohortMemberTable.IsActive)]
    [SqlBuilderProperty(CohortMemberTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Khoá học</summary>
    public Cohort? Cohort { get; set; }

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id         = Id,
        cohort_id  = CohortId,
        student_id = StudentId,
        section    = Section,
        joined_at  = JoinedAt,
        is_active  = IsActive
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id        = Id,
        section   = Section,
        is_active = IsActive
    };
}
