using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>
/// Lưu nguyên tử kết quả sinh đề: INSERT exam → INSERT exam_questions → UPDATE usage_count.
/// </summary>
public interface IExamGeneratorRepository
{
    /// <summary>
    /// Lưu nguyên tử: INSERT exam → INSERT exam_questions (snapshot) → UPDATE usage_count.
    /// Rollback toàn bộ nếu bất kỳ bước nào thất bại.
    /// </summary>
    Task<Guid> SaveExamAsync(
        Exam exam,
        IReadOnlyList<ExamQuestion> questions,
        IReadOnlySet<Guid> usedQuestionIds,
        CancellationToken ct = default);
}

