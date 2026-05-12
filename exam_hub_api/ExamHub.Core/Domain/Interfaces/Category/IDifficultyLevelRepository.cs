using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho DifficultyLevel</summary>
public interface IDifficultyLevelRepository : ICategoryRepository<DifficultyLevel, int>
{
    /// <summary>Tìm theo mã</summary>
    Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default);
}
