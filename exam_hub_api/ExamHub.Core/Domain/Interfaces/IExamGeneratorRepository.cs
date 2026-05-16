using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Repository cho việc sinh đề thi tự động.</summary>
public interface IExamGeneratorRepository
{
    /// <summary>
    /// Lấy ngẫu nhiên <paramref name="count"/> câu hỏi từ pool theo topic, độ khó và loại câu hỏi.
    /// Dùng ORDER BY RANDOM() — hiệu quả với covering index trên bảng questions.
    /// </summary>
    Task<IReadOnlyList<PickedQuestion>> PickRandomAsync(
        int topicId,
        int? questionTypeId,
        int difficultyId,
        int count,
        IReadOnlySet<Guid> excludeIds,
        CancellationToken ct = default);

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

