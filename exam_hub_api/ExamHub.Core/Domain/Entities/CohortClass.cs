using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Lớp học — được sinh tự động từ khoá qua DB trigger.
/// Mỗi dòng đại diện một lớp trong một năm học cụ thể.
/// </summary>
[Table(CohortClassTable.TableName)]
[SqlBuilderProperty(CohortClassTable.TableName)]
public class CohortClass : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty("id", Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Khóa ngoại khoá học</summary>
    [Column(CohortClassTable.CohortId)]
    [SqlBuilderProperty(CohortClassTable.CohortId, Insert = true, Update = false)]
    public int CohortId { get; set; }

    /// <summary>Khóa ngoại khối lớp (grade_levels)</summary>
    [Column(CohortClassTable.GradeLevelId)]
    [SqlBuilderProperty(CohortClassTable.GradeLevelId, Insert = true, Update = false)]
    public int GradeLevelId { get; set; }

    /// <summary>Tên lớp: 1A, 10A, ...</summary>
    [Column(CohortClassTable.ClassName)]
    [SqlBuilderProperty(CohortClassTable.ClassName, Insert = true, Update = false)]
    public required string ClassName { get; set; }

    /// <summary>Năm học: 2020-2021, 2021-2022, ...</summary>
    [Column(CohortClassTable.SchoolYear)]
    [SqlBuilderProperty(CohortClassTable.SchoolYear, Insert = true, Update = false)]
    public required string SchoolYear { get; set; }

    /// <summary>Năm thứ mấy của khoá (1, 2, 3, ...)</summary>
    [Column(CohortClassTable.YearIndex)]
    [SqlBuilderProperty(CohortClassTable.YearIndex, Insert = true, Update = false)]
    public short YearIndex { get; set; }

    /// <summary>ID giáo viên chủ nhiệm (nullable, có thể thay đổi mỗi năm)</summary>
    [Column(CohortClassTable.HomeroomTeacherId)]
    [SqlBuilderProperty(CohortClassTable.HomeroomTeacherId, Insert = true, Update = true)]
    public Guid? HomeroomTeacherId { get; set; }

    /// <summary>Thời điểm tạo</summary>
    [Column(CohortClassTable.CreatedAt)]
    [SqlBuilderProperty(CohortClassTable.CreatedAt, Insert = true, Update = false)]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Khoá học</summary>
    public Cohort? Cohort { get; set; }

    /// <summary>Khối lớp</summary>
    public GradeLevel? GradeLevel { get; set; }

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        cohort_id           = CohortId,
        grade_level_id      = GradeLevelId,
        class_name          = ClassName,
        school_year         = SchoolYear,
        year_index          = YearIndex,
        homeroom_teacher_id = HomeroomTeacherId,
        created_at          = CreatedAt
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id                  = Id,
        homeroom_teacher_id = HomeroomTeacherId
    };
}
