using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho CognitiveLevel (Bloom's Taxonomy)</summary>
public class CognitiveLevelService : ICognitiveLevelService
{
    private readonly ICognitiveLevelRepository _repo;
    public CognitiveLevelService(ICognitiveLevelRepository repo) => _repo = repo;

    public Task<IReadOnlyList<CognitiveLevel>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<CognitiveLevel>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<CognitiveLevel?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<CognitiveLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _repo.GetByCodeAsync(code, ct);

    public Task<CognitiveLevel> CreateAsync(CognitiveLevel entity, CancellationToken ct = default)
        => _repo.AddAsync(entity, ct);

    public async Task<CognitiveLevel> UpdateAsync(CognitiveLevel entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
