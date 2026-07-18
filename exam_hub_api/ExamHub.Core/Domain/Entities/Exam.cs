using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.FieldTables;
using TVT.Core.Models;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Đề thi cụ thể — được sinh từ ExamTemplate hoặc tạo thủ công
/// </summary>
[Table(ExamTable.TableName)]
[SqlBuilderProperty(ExamTable.TableName)]
public class Exam :ModifyModelBase, IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại mẫu đề thi (có thể null nếu tạo thủ công)</summary>
    [Column(ExamTable.ExamTemplateId)]
    [SqlBuilderProperty(ExamTable.ExamTemplateId, Insert = true, Update = true)]
    public Guid? ExamTemplateId { get; set; }

    /// <summary>Khóa ngoại lớp học</summary>
    [Column(ExamTable.GradeLevelId)]
    [SqlBuilderProperty(ExamTable.GradeLevelId, Insert = true, Update = true)]
    public int GradeLevelId { get; set; }

    /// <summary>Khóa ngoại môn học</summary>
    [Column(ExamTable.SubjectId)]
    [SqlBuilderProperty(ExamTable.SubjectId, Insert = true, Update = true)]
    public int SubjectId { get; set; }

    /// <summary>Tiêu đề đề thi</summary>
    [Column(ExamTable.Title)]
    [SqlBuilderProperty(ExamTable.Title, Insert = true, Update = true)]
    public required string Title { get; set; }

    /// <summary>Mã đề thi (DE_001)</summary>
    [Column(ExamTable.ExamCode)]
    [SqlBuilderProperty(ExamTable.ExamCode, Insert = true, Update = true)]
    public string? ExamCode { get; set; }

    /// <summary>Thời gian làm bài (phút)</summary>
    [Column(ExamTable.DurationMinutes)]
    [SqlBuilderProperty(ExamTable.DurationMinutes, Insert = true, Update = true)]
    public int DurationMinutes { get; set; } = 45;

    /// <summary>Tổng điểm của đề</summary>
    [Column(ExamTable.TotalScore)]
    [SqlBuilderProperty(ExamTable.TotalScore, Insert = true, Update = true)]
    public decimal TotalScore { get; set; } = 10.0m;

    /// <summary>Hướng dẫn làm bài</summary>
    [Column(ExamTable.Instructions)]
    [SqlBuilderProperty(ExamTable.Instructions, Insert = true, Update = true)]
    public string? Instructions { get; set; }

    /// <summary>Trạng thái đề thi</summary>
    [Column(ExamTable.Status)]
    [SqlBuilderProperty(ExamTable.Status, Insert = true, Update = true)]
    public ExamStatusEnum Status { get; set; } = ExamStatusEnum.Draft;

    /// <summary>Năm học (VD: "2024-2025")</summary>
    [Column(ExamTable.SchoolYear)]
    [SqlBuilderProperty(ExamTable.SchoolYear, Insert = true, Update = true)]
    public string? SchoolYear { get; set; }

    /// <summary>Học kỳ (1 hoặc 2)</summary>
    [Column(ExamTable.Semester)]
    [SqlBuilderProperty(ExamTable.Semester, Insert = true, Update = true)]
    public short? Semester { get; set; }

    /// <summary>Ngày thi</summary>
    [Column(ExamTable.ExamDate)]
    [SqlBuilderProperty(ExamTable.ExamDate, Insert = true, Update = true)]
    public DateOnly? ExamDate { get; set; }

    /// <summary>Tên lớp thi</summary>
    [Column(ExamTable.ClassName)]
    [SqlBuilderProperty(ExamTable.ClassName, Insert = true, Update = true)]
    public string? ClassName { get; set; }

    /// <summary>ID đề thi cha (dùng khi sinh đề theo lô)</summary>
    [Column(ExamTable.ParentExamId)]
    [SqlBuilderProperty(ExamTable.ParentExamId, Insert = true, Update = false)]
    public Guid? ParentExamId { get; set; }

    /// <summary>Chỉ số biến thể trong lô (001, 002...)</summary>
    [Column(ExamTable.VariantIndex)]
    [SqlBuilderProperty(ExamTable.VariantIndex, Insert = true, Update = false)]
    public short? VariantIndex { get; set; }

    /// <summary>UUID nhóm lô sinh đề</summary>
    [Column(ExamTable.BatchId)]
    [SqlBuilderProperty(ExamTable.BatchId, Insert = true, Update = false)]
    public Guid? BatchId { get; set; }

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Mẫu đề thi gốc</summary>
    public ExamTemplate? ExamTemplate { get; set; }

    /// <summary>Lớp học</summary>
    public GradeLevel? GradeLevel { get; set; }

    /// <summary>Môn học</summary>
    public Subject? Subject { get; set; }

    /// <summary>Đề thi cha (khi sinh theo lô)</summary>
    public Exam? ParentExam { get; set; }

    /// <summary>Danh sách đề thi con (biến thể)</summary>
    public List<Exam> Variants { get; set; } = [];

    /// <summary>Danh sách câu hỏi trong đề (snapshot)</summary>
    public List<ExamQuestion> Questions { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id               = Id,
        exam_template_id = ExamTemplateId,
        grade_level_id   = GradeLevelId,
        subject_id       = SubjectId,
        title            = Title,
        exam_code        = ExamCode,
        duration_minutes = DurationMinutes,
        total_score      = TotalScore,
        instructions     = Instructions,
        status           = Status.ToString().ToLower(),
        school_year      = SchoolYear,
        semester         = Semester,
        exam_date        = ExamDate,
        class_name       = ClassName,
        parent_exam_id   = ParentExamId,
        variant_index    = VariantIndex,
        batch_id         = BatchId,
        created = Created,
        modified = Modified
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id               = Id,
        grade_level_id   = GradeLevelId,
        subject_id       = SubjectId,
        title            = Title,
        exam_code        = ExamCode,
        duration_minutes = DurationMinutes,
        total_score      = TotalScore,
        instructions     = Instructions,
        status           = Status.ToString().ToLower(),
        school_year      = SchoolYear,
        semester         = Semester,
        exam_date        = ExamDate,
        class_name       = ClassName,
        modified       = DateTime.UtcNow
    };
}
