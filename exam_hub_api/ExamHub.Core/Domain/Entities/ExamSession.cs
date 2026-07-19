using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.FieldTables;
using TVT.Core.Models;

namespace ExamHub.Core.Domain.Entities;

/// <summary>Kỳ thi — cấu hình thi theo môn + cấp lớp, giao cho lớp/khoá.</summary>
[Table(ExamSessionTable.TableName)]
[SqlBuilderProperty(ExamSessionTable.TableName)]
public class ExamSession : ModifyModelBase, IModelBaseSql<Guid>
{
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(ExamSessionTable.Title)]
    [SqlBuilderProperty(ExamSessionTable.Title, Insert = true, Update = true)]
    public required string Title { get; set; }

    [Column(ExamSessionTable.Description)]
    [SqlBuilderProperty(ExamSessionTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    [Column(ExamSessionTable.SubjectId)]
    [SqlBuilderProperty(ExamSessionTable.SubjectId, Insert = true, Update = true)]
    public int SubjectId { get; set; }

    [Column(ExamSessionTable.GradeLevelId)]
    [SqlBuilderProperty(ExamSessionTable.GradeLevelId, Insert = true, Update = true)]
    public int GradeLevelId { get; set; }

    [Column(ExamSessionTable.OpenAt)]
    [SqlBuilderProperty(ExamSessionTable.OpenAt, Insert = true, Update = true)]
    public DateTime OpenAt { get; set; }

    [Column(ExamSessionTable.CloseAt)]
    [SqlBuilderProperty(ExamSessionTable.CloseAt, Insert = true, Update = true)]
    public DateTime CloseAt { get; set; }

    [Column(ExamSessionTable.MaxAttempts)]
    [SqlBuilderProperty(ExamSessionTable.MaxAttempts, Insert = true, Update = true)]
    public short MaxAttempts { get; set; } = 1;

    [Column(ExamSessionTable.PickMode)]
    [SqlBuilderProperty(ExamSessionTable.PickMode, Insert = true, Update = true)]
    public ExamSessionPickModeEnum PickMode { get; set; } = ExamSessionPickModeEnum.Random;

    [Column(ExamSessionTable.Status)]
    [SqlBuilderProperty(ExamSessionTable.Status, Insert = true, Update = true)]
    public ExamSessionStatusEnum Status { get; set; } = ExamSessionStatusEnum.Draft;

    // ── Navigation ──
    public Subject? Subject { get; set; }
    public GradeLevel? GradeLevel { get; set; }
    public List<ExamSessionExam> Exams { get; set; } = [];
    public List<ExamSessionAssignment> Assignments { get; set; } = [];

    public object ToInsertObject() => new
    {
        id = Id, title = Title, description = Description,
        subject_id = SubjectId, grade_level_id = GradeLevelId,
        open_at = OpenAt, close_at = CloseAt, max_attempts = MaxAttempts,
        pick_mode = PickMode.ToString(), status = Status.ToString().ToLower(),
        created = Created, created_by = CreatedBy, modified = Modified, modified_by = ModifiedBy
    };

    public object ToUpdateObject() => new
    {
        id = Id, title = Title, description = Description,
        subject_id = SubjectId, grade_level_id = GradeLevelId,
        open_at = OpenAt, close_at = CloseAt, max_attempts = MaxAttempts,
        pick_mode = PickMode.ToString(), status = Status.ToString().ToLower(),
        modified = DateTime.UtcNow, modified_by = ModifiedBy
    };
}
