using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho DifficultyLevel</summary>
public class DifficultyLevelService : IDifficultyLevelService
{
    private readonly IDifficultyLevelRepository _repo;
    public DifficultyLevelService(IDifficultyLevelRepository repo) => _repo = repo;

    public Task<IReadOnlyList<DifficultyLevel>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<DifficultyLevel>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<DifficultyLevel?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _repo.GetByCodeAsync(code, ct);

    public Task<DifficultyLevel> CreateAsync(DifficultyLevel entity, CancellationToken ct = default)
        => _repo.AddAsync(entity, ct);

    public async Task<DifficultyLevel> UpdateAsync(DifficultyLevel entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
