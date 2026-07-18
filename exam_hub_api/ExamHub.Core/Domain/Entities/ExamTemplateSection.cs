 
using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;
using TVT.Core.Models;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Cấu hình một phần trong mẫu đề thi
/// (VD: Phần I – Trắc nghiệm, Phần II – Tự luận)
/// </summary>
[Table(ExamTemplateSectionTable.TableName)]
[SqlBuilderProperty(ExamTemplateSectionTable.TableName)]
public class ExamTemplateSection :ModifyModelBase, IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại mẫu đề thi</summary>
    [Column(ExamTemplateSectionTable.ExamTemplateId)]
    [SqlBuilderProperty(ExamTemplateSectionTable.ExamTemplateId, Insert = true, Update = false)]
    public Guid ExamTemplateId { get; set; }

    /// <summary>Khóa ngoại chủ đề (null = lấy toàn bộ môn học)</summary>
    [Column(ExamTemplateSectionTable.TopicId)]
    [SqlBuilderProperty(ExamTemplateSectionTable.TopicId, Insert = true, Update = true)]
    public int? TopicId { get; set; }

    /// <summary>Khóa ngoại loại câu hỏi (null = tất cả loại)</summary>
    [Column(ExamTemplateSectionTable.QuestionTypeId)]
    [SqlBuilderProperty(ExamTemplateSectionTable.QuestionTypeId, Insert = true, Update = true)]
    public int? QuestionTypeId { get; set; }

    /// <summary>Lọc pool theo cấp độ nhận thức Bloom (null = không lọc)</summary>
    [Column(ExamTemplateSectionTable.CognitiveLevelId)]
    [SqlBuilderProperty(ExamTemplateSectionTable.CognitiveLevelId, Insert = true, Update = true)]
    public int? CognitiveLevelId { get; set; }

    /// <summary>Tên phần thi</summary>
    [Column(ExamTemplateSectionTable.SectionName)]
    [SqlBuilderProperty(ExamTemplateSectionTable.SectionName, Insert = true, Update = true)]
    public string? SectionName { get; set; }

    /// <summary>Số lượng câu hỏi trong phần</summary>
    [Column(ExamTemplateSectionTable.QuestionCount)]
    [SqlBuilderProperty(ExamTemplateSectionTable.QuestionCount, Insert = true, Update = true)]
    public int QuestionCount { get; set; }

    /// <summary>Điểm cho mỗi câu trong phần</summary>
    [Column(ExamTemplateSectionTable.ScorePerQuestion)]
    [SqlBuilderProperty(ExamTemplateSectionTable.ScorePerQuestion, Insert = true, Update = true)]
    public decimal? ScorePerQuestion { get; set; }

    /// <summary>Thứ tự phần trong đề</summary>
    [Column(ExamTemplateSectionTable.SortOrder)]
    [SqlBuilderProperty(ExamTemplateSectionTable.SortOrder, Insert = true, Update = true)]
    public short SortOrder { get; set; } = 0;

    /// <summary>Tỉ lệ % câu dễ (0–100)</summary>
    [Column(ExamTemplateSectionTable.PctEasy)]
    [SqlBuilderProperty(ExamTemplateSectionTable.PctEasy, Insert = true, Update = true)]
    public short PctEasy { get; set; } = 0;

    /// <summary>Tỉ lệ % câu trung bình (0–100)</summary>
    [Column(ExamTemplateSectionTable.PctMedium)]
    [SqlBuilderProperty(ExamTemplateSectionTable.PctMedium, Insert = true, Update = true)]
    public short PctMedium { get; set; } = 0;

    /// <summary>Tỉ lệ % câu khó (0–100)</summary>
    [Column(ExamTemplateSectionTable.PctHard)]
    [SqlBuilderProperty(ExamTemplateSectionTable.PctHard, Insert = true, Update = true)]
    public short PctHard { get; set; } = 0;

    /// <summary>Tỉ lệ % câu rất khó (0–100)</summary>
    [Column(ExamTemplateSectionTable.PctVeryHard)]
    [SqlBuilderProperty(ExamTemplateSectionTable.PctVeryHard, Insert = true, Update = true)]
    public short PctVeryHard { get; set; } = 0;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Mẫu đề thi</summary>
    public ExamTemplate? ExamTemplate { get; set; }

    /// <summary>Chủ đề được chỉ định</summary>
    public Topic? Topic { get; set; }

    /// <summary>Loại câu hỏi được chỉ định</summary>
    public QuestionType? QuestionType { get; set; }

    /// <summary>Cấp độ nhận thức Bloom dùng để lọc pool</summary>
    public CognitiveLevel? CognitiveLevel { get; set; }

    // ── Domain Logic ────────────────────────────────────────────
    /// <summary>
    /// Xác nhận tổng % độ khó có hợp lệ (bằng 100) không
    /// </summary>
    public bool IsDifficultyDistributionValid() =>
        PctEasy + PctMedium + PctHard + PctVeryHard == 100;

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id                  = Id,
        exam_template_id    = ExamTemplateId,
        topic_id            = TopicId,
        question_type_id    = QuestionTypeId,
        cognitive_level_id  = CognitiveLevelId,
        section_name        = SectionName,
        question_count      = QuestionCount,
        score_per_question  = ScorePerQuestion,
        sort_order          = SortOrder,
        pct_easy            = PctEasy,
        pct_medium          = PctMedium,
        pct_hard            = PctHard,
        pct_very_hard       = PctVeryHard,
        created_at          = Created
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id                  = Id,
        topic_id            = TopicId,
        question_type_id    = QuestionTypeId,
        cognitive_level_id  = CognitiveLevelId,
        section_name        = SectionName,
        question_count      = QuestionCount,
        score_per_question  = ScorePerQuestion,
        sort_order          = SortOrder,
        pct_easy            = PctEasy,
        pct_medium          = PctMedium,
        pct_hard            = PctHard,
        pct_very_hard       = PctVeryHard
    };
}
