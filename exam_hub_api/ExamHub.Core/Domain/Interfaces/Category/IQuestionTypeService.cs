using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho QuestionType</summary>
public interface IQuestionTypeService : ICategoryService<QuestionType, int>
{
    /// <summary>Lấy theo mã</summary>
    Task<QuestionType?> GetByCodeAsync(string code, CancellationToken ct = default);
}
