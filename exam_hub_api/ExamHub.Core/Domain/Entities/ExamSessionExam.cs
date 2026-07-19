using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>Đề thi thuộc pool của một kỳ thi.</summary>
[Table(ExamSessionExamTable.TableName)]
[SqlBuilderProperty(ExamSessionExamTable.TableName)]
public class ExamSessionExam
{
    [Column(CommonFieldTable.Id)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(ExamSessionExamTable.SessionId)]
    public Guid SessionId { get; set; }

    [Column(ExamSessionExamTable.ExamId)]
    public Guid ExamId { get; set; }

    public Exam? Exam { get; set; }
}
