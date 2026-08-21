using System.Text.Json;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.Application.Grading;

/// <summary>Logic thuần quyết định chấm điểm — không phụ thuộc DB, unit-test được.</summary>
public static class SubmissionGrading
{
    /// <summary>Trích tập UUID đáp án đúng từ snapshot JSON [{id, is_correct, ...}].</summary>
    public static HashSet<Guid> CorrectAnswerIds(string? answersSnapshotJson)
    {
        var result = new HashSet<Guid>();
        if (string.IsNullOrWhiteSpace(answersSnapshotJson)) return result;

        using var doc = JsonDocument.Parse(answersSnapshotJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Array) return result;

        foreach (var el in doc.RootElement.EnumerateArray())
            if (el.TryGetProperty("is_correct", out var ic) && ic.ValueKind == JsonValueKind.True &&
                el.TryGetProperty("id", out var idEl) && idEl.TryGetGuid(out var id))
                result.Add(id);
        return result;
    }

    /// <summary>Câu chấm tay = không có đáp án đúng nào trong snapshot (tự luận).</summary>
    public static bool HasManualGradeQuestion(IEnumerable<ExamQuestion> examQuestions)
        => examQuestions.Any(eq => CorrectAnswerIds(eq.AnswersSnapshot).Count == 0);

    /// <summary>PendingManualGrade nếu có câu tự luận, ngược lại Graded.</summary>
    public static SubmissionStatusEnum DecideStatus(IEnumerable<ExamQuestion> examQuestions)
        => HasManualGradeQuestion(examQuestions)
            ? SubmissionStatusEnum.PendingManualGrade
            : SubmissionStatusEnum.Graded;
}
