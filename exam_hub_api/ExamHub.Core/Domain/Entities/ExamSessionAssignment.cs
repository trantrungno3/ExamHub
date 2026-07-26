using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>Giao kỳ thi cho một lớp (cohort_class) hoặc một khoá (cohort).</summary>
[Table(ExamSessionAssignmentTable.TableName)]
[SqlBuilderProperty(ExamSessionAssignmentTable.TableName)]
public class ExamSessionAssignment
{
    [Column(CommonFieldTable.Id)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(ExamSessionAssignmentTable.SessionId)]
    public Guid SessionId { get; set; }

    [Column(ExamSessionAssignmentTable.CohortId)]
    public int? CohortId { get; set; }

    [Column(ExamSessionAssignmentTable.CohortClassId)]
    public int? CohortClassId { get; set; }

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Khoá được giao (khi giao cả khoá)</summary>
    public Cohort? Cohort { get; set; }

    /// <summary>Lớp được giao (khi giao một lớp cụ thể)</summary>
    public CohortClass? CohortClass { get; set; }
}
