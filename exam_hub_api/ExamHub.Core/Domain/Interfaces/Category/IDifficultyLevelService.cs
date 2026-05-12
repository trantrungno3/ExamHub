using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Service interface cho DifficultyLevel</summary>
public interface IDifficultyLevelService : ICategoryService<DifficultyLevel, int>
{
    /// <summary>Lấy theo mã</summary>
    Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default);
}
