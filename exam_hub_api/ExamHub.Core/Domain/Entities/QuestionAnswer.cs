using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Đáp án của một câu hỏi
/// </summary>
[Table(QuestionAnswerTable.TableName)]
[SqlBuilderProperty(QuestionAnswerTable.TableName)]
public class QuestionAnswer : IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại câu hỏi</summary>
    [Column(QuestionAnswerTable.QuestionId)]
    [SqlBuilderProperty(QuestionAnswerTable.QuestionId, Insert = true, Update = false)]
    public Guid QuestionId { get; set; }

    /// <summary>Nội dung đáp án (HTML/Markdown)</summary>
    [Column(QuestionAnswerTable.Content)]
    [SqlBuilderProperty(QuestionAnswerTable.Content, Insert = true, Update = true)]
    public required string Content { get; set; }

    /// <summary>Nội dung thuần text</summary>
    [Column(QuestionAnswerTable.ContentPlain)]
    [SqlBuilderProperty(QuestionAnswerTable.ContentPlain, Insert = true, Update = true)]
    public string? ContentPlain { get; set; }

    /// <summary>Đây là đáp án đúng hay không</summary>
    [Column(QuestionAnswerTable.IsCorrect)]
    [SqlBuilderProperty(QuestionAnswerTable.IsCorrect, Insert = true, Update = true)]
    public bool IsCorrect { get; set; } = false;

    /// <summary>Thứ tự hiển thị (A, B, C, D...)</summary>
    [Column(QuestionAnswerTable.SortOrder)]
    [SqlBuilderProperty(QuestionAnswerTable.SortOrder, Insert = true, Update = true)]
    public short SortOrder { get; set; } = 0;

    /// <summary>Giải thích riêng cho đáp án này</summary>
    [Column(QuestionAnswerTable.Explanation)]
    [SqlBuilderProperty(QuestionAnswerTable.Explanation, Insert = true, Update = true)]
    public string? Explanation { get; set; }

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Câu hỏi chứa đáp án này</summary>
    public Question? Question { get; set; }

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id            = Id,
        question_id   = QuestionId,
        content       = Content,
        content_plain = ContentPlain,
        is_correct    = IsCorrect,
        sort_order    = SortOrder,
        explanation   = Explanation
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id            = Id,
        content       = Content,
        content_plain = ContentPlain,
        is_correct    = IsCorrect,
        sort_order    = SortOrder,
        explanation   = Explanation
    };
}
