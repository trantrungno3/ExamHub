using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho QuestionType</summary>
public interface IQuestionTypeRepository : ICategoryRepository<QuestionType, int>
{
    /// <summary>Tìm theo mã</summary>
    Task<QuestionType?> GetByCodeAsync(string code, CancellationToken ct = default);
}
