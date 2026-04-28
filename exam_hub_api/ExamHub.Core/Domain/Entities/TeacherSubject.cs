using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Quan hệ giáo viên — môn học phụ trách
/// </summary>
[Table(TeacherSubjectTable.TableName)]
[SqlBuilderProperty(TeacherSubjectTable.TableName)]
public class TeacherSubject : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Khóa ngoại người dùng (giáo viên)</summary>
    [Column(TeacherSubjectTable.UserId)]
    [SqlBuilderProperty(TeacherSubjectTable.UserId, Insert = true, Update = false)]
    public Guid UserId { get; set; }

    /// <summary>Khóa ngoại môn học</summary>
    [Column(TeacherSubjectTable.SubjectId)]
    [SqlBuilderProperty(TeacherSubjectTable.SubjectId, Insert = true, Update = false)]
    public int SubjectId { get; set; }

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Môn học</summary>
    public Subject? Subject { get; set; }

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        user_id    = UserId,
        subject_id = SubjectId
    };

    /// <summary>Không hỗ trợ cập nhật — xóa và tạo mới</summary>
    public object ToUpdateObject() => new { id = Id };
}
