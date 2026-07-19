using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.FieldTables;
using TVT.Core.Models;

namespace ExamHub.Core.Domain.Entities;

/// <summary>
/// Bài nộp của học sinh cho một đề thi
/// </summary>
[Table(ExamSubmissionTable.TableName)]
[SqlBuilderProperty(ExamSubmissionTable.TableName)]
public class ExamSubmission : ModifyModelBase, IModelBaseSql<Guid>
{
    /// <summary>Khóa chính (UUID)</summary>
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khóa ngoại đề thi</summary>
    [Column(ExamSubmissionTable.ExamId)]
    [SqlBuilderProperty(ExamSubmissionTable.ExamId, Insert = true, Update = false)]
    public Guid ExamId { get; set; }

    /// <summary>Khóa ngoại học sinh</summary>
    [Column(ExamSubmissionTable.StudentId)]
    [SqlBuilderProperty(ExamSubmissionTable.StudentId, Insert = true, Update = false)]
    public Guid StudentId { get; set; }

    /// <summary>Kỳ thi (null = đề trực tiếp, luồng cũ)</summary>
    [Column(ExamSubmissionTable.SessionId)]
    [SqlBuilderProperty(ExamSubmissionTable.SessionId, Insert = true, Update = false)]
    public Guid? SessionId { get; set; }

    /// <summary>Số thứ tự lượt làm trong kỳ thi</summary>
    [Column(ExamSubmissionTable.AttemptNo)]
    [SqlBuilderProperty(ExamSubmissionTable.AttemptNo, Insert = true, Update = false)]
    public short AttemptNo { get; set; } = 1;

    /// <summary>Thời điểm bắt đầu làm bài</summary>
    [Column(ExamSubmissionTable.StartedAt)]
    [SqlBuilderProperty(ExamSubmissionTable.StartedAt, Insert = true, Update = false)]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời điểm nộp bài</summary>
    [Column(ExamSubmissionTable.SubmittedAt)]
    [SqlBuilderProperty(ExamSubmissionTable.SubmittedAt, Insert = true, Update = true)]
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Tổng thời gian làm bài tính bằng giây</summary>
    [Column(ExamSubmissionTable.DurationSeconds)]
    [SqlBuilderProperty(ExamSubmissionTable.DurationSeconds, Insert = true, Update = true)]
    public int? DurationSeconds { get; set; }

    /// <summary>Tổng điểm đạt được</summary>
    [Column(ExamSubmissionTable.TotalScore)]
    [SqlBuilderProperty(ExamSubmissionTable.TotalScore, Insert = true, Update = true)]
    public decimal? TotalScore { get; set; }

    /// <summary>Học sinh có vượt điểm đạt hay không</summary>
    [Column(ExamSubmissionTable.IsPassed)]
    [SqlBuilderProperty(ExamSubmissionTable.IsPassed, Insert = true, Update = true)]
    public bool? IsPassed { get; set; }

    /// <summary>Trạng thái bài nộp</summary>
    [Column(ExamSubmissionTable.Status)]
    [SqlBuilderProperty(ExamSubmissionTable.Status, Insert = true, Update = true)]
    public SubmissionStatusEnum Status { get; set; } = SubmissionStatusEnum.InProgress;

    // ── Navigation ──────────────────────────────────────────────
    /// <summary>Đề thi</summary>
    public Exam? Exam { get; set; }

    /// <summary>Danh sách câu trả lời chi tiết</summary>
    public List<SubmissionAnswer> Answers { get; set; } = [];

    // ── Helpers ─────────────────────────────────────────────────
    /// <summary>Tạo object để INSERT</summary>
    public object ToInsertObject() => new
    {
        id = Id,
        exam_id = ExamId,
        student_id = StudentId,
        session_id = SessionId,
        attempt_no = AttemptNo,
        started_at = StartedAt,
        submitted_at = SubmittedAt,
        duration_seconds = DurationSeconds,
        total_score = TotalScore,
        is_passed = IsPassed,
        status = Status.ToString().ToLower(),
        created_by = CreatedBy,
        modified_by = ModifiedBy
    };

    /// <summary>Tạo object để UPDATE</summary>
    public object ToUpdateObject() => new
    {
        id = Id,
        submitted_at = SubmittedAt,
        duration_seconds = DurationSeconds,
        total_score = TotalScore,
        is_passed = IsPassed,
        status = Status.ToString().ToLower(),
        modified = Modified,
        modified_by = ModifiedBy,
    };
}