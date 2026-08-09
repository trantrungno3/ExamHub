using System.ComponentModel.DataAnnotations;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.DataTransferObjects.ExamSession;

/// <summary>Request tạo kỳ thi.</summary>
public sealed record CreateExamSessionRequest
{
    [Required, MaxLength(300)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int SubjectId { get; set; }
    [Range(1, int.MaxValue)] public int GradeLevelId { get; set; }
    [Required] public DateTime OpenAt { get; set; }
    [Required] public DateTime CloseAt { get; set; }
    [Range(1, 100)] public short MaxAttempts { get; set; } = 1;
    [RegularExpression("^(Random|StudentChoice)$")] public string PickMode { get; set; } = "Random";

    public Domain.Entities.ExamSession ToEntity() => new()
    {
        Title = Title, Description = Description, SubjectId = SubjectId, GradeLevelId = GradeLevelId,
        OpenAt = OpenAt.ToUniversalTime(), CloseAt = CloseAt.ToUniversalTime(), MaxAttempts = MaxAttempts,
        PickMode = Enum.Parse<ExamSessionPickModeEnum>(PickMode),
        Status = ExamSessionStatusEnum.Draft
    };
}

/// <summary>Request cập nhật kỳ thi (không đổi trạng thái ở đây).</summary>
public sealed record UpdateExamSessionRequest
{
    [Required, MaxLength(300)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int SubjectId { get; set; }
    [Range(1, int.MaxValue)] public int GradeLevelId { get; set; }
    [Required] public DateTime OpenAt { get; set; }
    [Required] public DateTime CloseAt { get; set; }
    [Range(1, 100)] public short MaxAttempts { get; set; } = 1;
    [RegularExpression("^(Random|StudentChoice)$")] public string PickMode { get; set; } = "Random";
}

public sealed record SetSessionExamsRequest(IReadOnlyList<Guid> ExamIds);
public sealed record CreateAssignmentRequest(int? CohortId, int? CohortClassId);
public sealed record StartSessionRequest(Guid? ExamId);

/// <summary>Tóm tắt kỳ thi cho danh sách quản lý.</summary>
public sealed record ExamSessionResponse(
    Guid Id, string Title, int SubjectId, string? SubjectName,
    int GradeLevelId, string? GradeLevelName,
    long OpenAt, long CloseAt, short MaxAttempts, string PickMode, string Status,
    int ExamCount, int AssignmentCount)
{
    public static ExamSessionResponse FromEntity(Domain.Entities.ExamSession s) => new(
        s.Id, s.Title, s.SubjectId, s.Subject?.Name, s.GradeLevelId, s.GradeLevel?.Name,
        new DateTimeOffset(s.OpenAt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
        new DateTimeOffset(s.CloseAt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
        s.MaxAttempts, s.PickMode.ToString(), s.Status.ToString().ToLower(),
        s.Exams.Count, s.Assignments.Count);
}

public sealed record SessionExamResponse(Guid ExamId, string Title, string? ExamCode, decimal TotalScore);

/// <summary>Chi tiết kỳ thi kèm pool đề + assignments.</summary>
public sealed record ExamSessionDetailResponse(
    Guid Id, string Title, string? Description, int SubjectId, string? SubjectName,
    int GradeLevelId, string? GradeLevelName, long OpenAt, long CloseAt,
    short MaxAttempts, string PickMode, string Status,
    IReadOnlyList<SessionExamResponse> Exams,
    IReadOnlyList<AssignmentResponse> Assignments);

public sealed record AssignmentResponse(Guid Id, int? CohortId, string? CohortName, int? CohortClassId, string? CohortClassName, string? SchoolName, int StudentCount);

/// <summary>Kỳ thi được giao — hiển thị phía học sinh.</summary>
public sealed record MySessionResponse(
    Guid Id, string Title, string? SubjectName, string? GradeLevelName,
    long OpenAt, long CloseAt, string PickMode, string Availability,
    short MaxAttempts, int UsedAttempts,
    Guid? InProgressSubmissionId, Guid? InProgressExamId);

/// <summary>Một đề trong pool + trạng thái của học sinh (dùng cho student_choice).</summary>
public sealed record SessionPoolItemResponse(
    Guid ExamId, string Title, string? ExamCode, decimal TotalScore,
    string StudentState, Guid? SubmissionId);

public sealed record StartSessionResponse(Guid SubmissionId, Guid ExamId);
