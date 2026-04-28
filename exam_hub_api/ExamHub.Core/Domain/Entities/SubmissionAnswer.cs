using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Câu trả lời chi tiết của học sinh cho từng câu trong bài nộp
/// </summary>
[Table(SubmissionAnswerTable.TableName)]
[SqlBuilderProperty(SubmissionAnswerTable.TableName)]
public class SubmissionAnswer : IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại bài nộp</summary>
    [Column(SubmissionAnswerTable.SubmissionId)]
    [SqlBuilderProperty(SubmissionAnswerTable.SubmissionId, Insert = true, Update = false)]
    public Guid SubmissionId { get; set; }

    /// <summary>Khóa ngoại câu hỏi trong đề thi (snapshot)</summary>
    [Column(SubmissionAnswerTable.ExamQuestionId)]
    [SqlBuilderProperty(SubmissionAnswerTable.ExamQuestionId, Insert = true, Update = false)]
    public Guid ExamQuestionId { get; set; }

    /// <summary>
    /// Danh sách UUID đáp án đã chọn
    /// (trắc nghiệm 1 đáp án: 1 phần tử, nhiều đáp án: nhiều phần tử)
    /// </summary>
    [Column(SubmissionAnswerTable.SelectedAnswerIds)]
    [SqlBuilderProperty(SubmissionAnswerTable.SelectedAnswerIds, Insert = true, Update = true)]
    public Guid[]? SelectedAnswerIds { get; set; }

    /// <summary>Nội dung câu tự luận</summary>
    [Column(SubmissionAnswerTable.EssayContent)]
    [SqlBuilderProperty(SubmissionAnswerTable.EssayContent, Insert = true, Update = true)]
    public string? EssayContent { get; set; }

    /// <summary>Kết quả đúng/sai (null = chưa chấm)</summary>
    [Column(SubmissionAnswerTable.IsCorrect)]
    [SqlBuilderProperty(SubmissionAnswerTable.IsCorrect, Insert = true, Update = true)]
    public bool? IsCorrect { get; set; }

    /// <summary>Điểm đạt được cho câu này</summary>
    [Column(SubmissionAnswerTable.ScoreEarned)]
    [SqlBuilderProperty(SubmissionAnswerTable.ScoreEarned, Insert = true, Update = true)]
    public decimal ScoreEarned { get; set; } = 0;

    /// <summary>Nhận xét / phản hồi từ giáo viên</summary>
    [Column(SubmissionAnswerTable.Feedback)]
    [SqlBuilderProperty(SubmissionAnswerTable.Feedback, Insert = true, Update = true)]
    public string? Feedback { get; set; }

    /// <summary>ID giáo viên chấm điểm câu tự luận này</summary>
    [Column(SubmissionAnswerTable.GradedBy)]
    [SqlBuilderProperty(SubmissionAnswerTable.GradedBy, Insert = true, Update = true)]
    public Guid? GradedBy { get; set; }

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Bài nộp chứa câu trả lời này</summary>
    public ExamSubmission? Submission { get; set; }

    /// <summary>Câu hỏi tương ứng trong đề thi</summary>
    public ExamQuestion? ExamQuestion { get; set; }

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id                  = Id,
        submission_id       = SubmissionId,
        exam_question_id    = ExamQuestionId,
        selected_answer_ids = SelectedAnswerIds,
        essay_content       = EssayContent,
        is_correct          = IsCorrect,
        score_earned        = ScoreEarned,
        feedback            = Feedback,
        graded_by           = GradedBy
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id                  = Id,
        selected_answer_ids = SelectedAnswerIds,
        essay_content       = EssayContent,
        is_correct          = IsCorrect,
        score_earned        = ScoreEarned,
        feedback            = Feedback,
        graded_by           = GradedBy
    };
}
