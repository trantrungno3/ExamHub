using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Câu hỏi được snapshot vào đề thi tại thời điểm sinh đề
/// (đảm bảo đề không thay đổi khi câu hỏi gốc bị sửa)
/// </summary>
[Table(ExamQuestionTable.TableName)]
[SqlBuilderProperty(ExamQuestionTable.TableName)]
public class ExamQuestion : IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại đề thi</summary>
    [Column(ExamQuestionTable.ExamId)]
    [SqlBuilderProperty(ExamQuestionTable.ExamId, Insert = true, Update = false)]
    public Guid ExamId { get; set; }

    /// <summary>Khóa ngoại câu hỏi gốc trong ngân hàng</summary>
    [Column(ExamQuestionTable.QuestionId)]
    [SqlBuilderProperty(ExamQuestionTable.QuestionId, Insert = true, Update = false)]
    public Guid QuestionId { get; set; }

    /// <summary>Tên phần thi chứa câu này</summary>
    [Column(ExamQuestionTable.SectionName)]
    [SqlBuilderProperty(ExamQuestionTable.SectionName, Insert = true, Update = true)]
    public string? SectionName { get; set; }

    /// <summary>Thứ tự câu hỏi trong đề</summary>
    [Column(ExamQuestionTable.SortOrder)]
    [SqlBuilderProperty(ExamQuestionTable.SortOrder, Insert = true, Update = true)]
    public int SortOrder { get; set; }

    /// <summary>Điểm của câu hỏi này</summary>
    [Column(ExamQuestionTable.Score)]
    [SqlBuilderProperty(ExamQuestionTable.Score, Insert = true, Update = true)]
    public decimal? Score { get; set; }

    /// <summary>
    /// Snapshot nội dung câu hỏi tại thời điểm sinh đề
    /// </summary>
    [Column(ExamQuestionTable.ContentSnapshot)]
    [SqlBuilderProperty(ExamQuestionTable.ContentSnapshot, Insert = true, Update = false)]
    public required string ContentSnapshot { get; set; }

    /// <summary>
    /// Snapshot danh sách đáp án (JSONB) — [{id, content, is_correct, sort_order, explanation}].
    /// <c>id</c> là UUID đáp án gốc; học sinh chọn theo id này và lưu vào
    /// <see cref="SubmissionAnswer.SelectedAnswerIds"/> để chấm trắc nghiệm tự động.
    /// </summary>
    [Column(ExamQuestionTable.AnswersSnapshot)]
    [SqlBuilderProperty(ExamQuestionTable.AnswersSnapshot, Insert = true, Update = false)]
    public string? AnswersSnapshot { get; set; }

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Đề thi chứa câu này</summary>
    public Exam? Exam { get; set; }

    /// <summary>Câu hỏi gốc trong ngân hàng</summary>
    public Question? Question { get; set; }

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id                = Id,
        exam_id           = ExamId,
        question_id       = QuestionId,
        section_name      = SectionName,
        sort_order        = SortOrder,
        score             = Score,
        content_snapshot  = ContentSnapshot,
        answers_snapshot  = AnswersSnapshot
    };

    /// <summary>Tạo object để UPDATE (chỉ sort_order và score)</summary>
    public object ToUpdateObject() => new
    {
        id           = Id,
        section_name = SectionName,
        sort_order   = SortOrder,
        score        = Score
    };
}
