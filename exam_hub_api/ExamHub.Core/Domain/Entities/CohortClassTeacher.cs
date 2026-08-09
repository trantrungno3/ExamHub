using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Phân công giáo viên giảng dạy cho lớp theo môn (1 môn/lớp = 1 GV).
/// </summary>
[Table(CohortClassTeacherTable.TableName)]
[SqlBuilderProperty(CohortClassTeacherTable.TableName)]
public class CohortClassTeacher : IModelBaseSql<int>
{
    /// <summary>Khóa chính (SERIAL)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = false, Update = false)]
    public int Id { get; set; }

    /// <summary>Khóa ngoại lớp học</summary>
    [Column(CohortClassTeacherTable.CohortClassId)]
    [SqlBuilderProperty(CohortClassTeacherTable.CohortClassId, Insert = true, Update = false)]
    public int CohortClassId { get; set; }

    /// <summary>Khóa ngoại môn học</summary>
    [Column(CohortClassTeacherTable.SubjectId)]
    [SqlBuilderProperty(CohortClassTeacherTable.SubjectId, Insert = true, Update = false)]
    public int SubjectId { get; set; }

    /// <summary>Khóa ngoại giáo viên</summary>
    [Column(CohortClassTeacherTable.TeacherId)]
    [SqlBuilderProperty(CohortClassTeacherTable.TeacherId, Insert = true, Update = false)]
    public Guid TeacherId { get; set; }

    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        cohort_class_id = CohortClassId,
        subject_id      = SubjectId,
        teacher_id      = TeacherId
    };

    /// <summary>Không hỗ trợ cập nhật — xóa và tạo mới</summary>
    public object ToUpdateObject() => new { id = Id };
}
