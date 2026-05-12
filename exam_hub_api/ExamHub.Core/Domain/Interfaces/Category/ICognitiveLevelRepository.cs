using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.Domain.Interfaces;

/// <summary>Interface repository cho CognitiveLevel (Bloom's Taxonomy)</summary>
public interface ICognitiveLevelRepository : ICategoryRepository<CognitiveLevel, int>
{
    /// <summary>Tìm theo mã (remember, understand, apply, ...)</summary>
    Task<CognitiveLevel?> GetByCodeAsync(string code, CancellationToken ct = default);
}
