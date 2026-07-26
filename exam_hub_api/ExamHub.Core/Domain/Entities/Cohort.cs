using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Khoá học — đơn vị tuyển sinh theo năm (VD: Khoá 2020-2025).
/// Khi INSERT, trigger DB tự sinh các dòng cohort_classes tương ứng.
/// </summary>
[Table(CohortTable.TableName)]
[SqlBuilderProperty(CohortTable.TableName)]
public class Cohort : ModifyModelBase, IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty("id", Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Khóa ngoại trường học</summary>
    [Column(CohortTable.SchoolId)]
    [SqlBuilderProperty(CohortTable.SchoolId, Insert = true, Update = false)]
    public int SchoolId { get; set; }

    /// <summary>Tên khoá: Khoá 2020-2025</summary>
    [Column(CohortTable.Name)]
    [SqlBuilderProperty(CohortTable.Name, Insert = true, Update = true)]
    public required string Name { get; set; }

    /// <summary>Năm học bắt đầu (VD: 2020)</summary>
    [Column(CohortTable.StartYear)]
    [SqlBuilderProperty(CohortTable.StartYear, Insert = true, Update = false)]
    public short StartYear { get; set; }

    /// <summary>Năm học kết thúc (VD: 2025)</summary>
    [Column(CohortTable.EndYear)]
    [SqlBuilderProperty(CohortTable.EndYear, Insert = true, Update = false)]
    public short EndYear { get; set; }

    /// <summary>Lớp bắt đầu của khoá (VD: 1 cho tiểu học, 10 cho THPT)</summary>
    [Column(CohortTable.GradeStart)]
    [SqlBuilderProperty(CohortTable.GradeStart, Insert = true, Update = false)]
    public short GradeStart { get; set; }

    /// <summary>Số lớp song song trong khoá (1..26 → A, B, C, ...)</summary>
    [Column(CohortTable.NumClasses)]
    [SqlBuilderProperty(CohortTable.NumClasses, Insert = true, Update = true)]
    public short NumClasses { get; set; } = 1;

    /// <summary>Kích hoạt hay không</summary>
    [Column(CohortTable.IsActive)]
    [SqlBuilderProperty(CohortTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Trường học</summary>
    public School? School { get; set; }

    /// <summary>Các lớp học được sinh từ khoá này</summary>
    public List<CohortClass> Classes { get; set; } = [];

    /// <summary>Danh sách học sinh trong khoá</summary>
    public List<CohortMember> Members { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        school_id    = SchoolId,
        name         = Name,
        start_year   = StartYear,
        end_year     = EndYear,
        grade_start  = GradeStart,
        num_classes  = NumClasses,
        is_active    = IsActive
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id           = Id,
        name         = Name,
        num_classes  = NumClasses,
        is_active    = IsActive
    };
}
