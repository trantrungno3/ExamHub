using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;
using TVT.Core.Models;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Mẫu đề thi — cấu hình để sinh đề thi tự động
/// </summary>
[Table(ExamTemplateTable.TableName)]
[SqlBuilderProperty(ExamTemplateTable.TableName)]
public class ExamTemplate : ModifyModelBase, IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại lớp học</summary>
    [Column(ExamTemplateTable.GradeLevelId)]
    [SqlBuilderProperty(ExamTemplateTable.GradeLevelId, Insert = true, Update = true)]
    public int GradeLevelId { get; set; }

    /// <summary>Khóa ngoại môn học</summary>
    [Column(ExamTemplateTable.SubjectId)]
    [SqlBuilderProperty(ExamTemplateTable.SubjectId, Insert = true, Update = true)]
    public int SubjectId { get; set; }

    /// <summary>Tiêu đề mẫu đề thi</summary>
    [Column(ExamTemplateTable.Title)]
    [SqlBuilderProperty(ExamTemplateTable.Title, Insert = true, Update = true)]
    public required string Title { get; set; }

    /// <summary>Mô tả mẫu đề thi</summary>
    [Column(ExamTemplateTable.Description)]
    [SqlBuilderProperty(ExamTemplateTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    /// <summary>Thời gian làm bài (phút)</summary>
    [Column(ExamTemplateTable.DurationMinutes)]
    [SqlBuilderProperty(ExamTemplateTable.DurationMinutes, Insert = true, Update = true)]
    public int DurationMinutes { get; set; } = 45;

    /// <summary>Tổng số câu hỏi</summary>
    [Column(ExamTemplateTable.TotalQuestions)]
    [SqlBuilderProperty(ExamTemplateTable.TotalQuestions, Insert = true, Update = true)]
    public int? TotalQuestions { get; set; }

    /// <summary>Tổng điểm</summary>
    [Column(ExamTemplateTable.TotalScore)]
    [SqlBuilderProperty(ExamTemplateTable.TotalScore, Insert = true, Update = true)]
    public decimal TotalScore { get; set; } = 10.0m;

    /// <summary>Xáo trộn câu hỏi khi sinh đề</summary>
    [Column(ExamTemplateTable.ShuffleQuestions)]
    [SqlBuilderProperty(ExamTemplateTable.ShuffleQuestions, Insert = true, Update = true)]
    public bool ShuffleQuestions { get; set; } = true;

    /// <summary>Xáo trộn đáp án khi sinh đề</summary>
    [Column(ExamTemplateTable.ShuffleAnswers)]
    [SqlBuilderProperty(ExamTemplateTable.ShuffleAnswers, Insert = true, Update = true)]
    public bool ShuffleAnswers { get; set; } = true;

    /// <summary>Ngăn trùng câu hỏi giữa các lần sinh đề</summary>
    [Column(ExamTemplateTable.PreventDuplicate)]
    [SqlBuilderProperty(ExamTemplateTable.PreventDuplicate, Insert = true, Update = true)]
    public bool PreventDuplicate { get; set; } = true;

    /// <summary>Hướng dẫn làm bài</summary>
    [Column(ExamTemplateTable.Instructions)]
    [SqlBuilderProperty(ExamTemplateTable.Instructions, Insert = true, Update = true)]
    public string? Instructions { get; set; }

    /// <summary>Kích hoạt hay không</summary>
    [Column(ExamTemplateTable.IsActive)]
    [SqlBuilderProperty(ExamTemplateTable.IsActive, Insert = true, Update = true)]
    public bool IsActive { get; set; } = true;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Lớp học</summary>
    public GradeLevel? GradeLevel { get; set; }

    /// <summary>Môn học</summary>
    public Subject? Subject { get; set; }

    /// <summary>Danh sách cấu hình phần thi</summary>
    public List<ExamTemplateSection> Sections { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id = Id,
        grade_level_id = GradeLevelId,
        subject_id = SubjectId,
        title = Title,
        description = Description,
        duration_minutes = DurationMinutes,
        total_questions = TotalQuestions,
        total_score = TotalScore,
        shuffle_questions = ShuffleQuestions,
        shuffle_answers = ShuffleAnswers,
        prevent_duplicate = PreventDuplicate,
        instructions = Instructions,
        is_active = IsActive,
        created = Created,
        modified = Modified
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id = Id,
        grade_level_id = GradeLevelId,
        subject_id = SubjectId,
        title = Title,
        description = Description,
        duration_minutes = DurationMinutes,
        total_questions = TotalQuestions,
        total_score = TotalScore,
        shuffle_questions = ShuffleQuestions,
        shuffle_answers = ShuffleAnswers,
        prevent_duplicate = PreventDuplicate,
        instructions = Instructions,
        is_active = IsActive,
        modified = DateTime.UtcNow
    };
}