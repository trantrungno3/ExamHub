using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho GradeLevel</summary>
public class GradeLevelService : IGradeLevelService
{
    private readonly IGradeLevelRepository _repo;
    public GradeLevelService(IGradeLevelRepository repo) => _repo = repo;

    public Task<IReadOnlyList<GradeLevel>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<GradeLevel>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<GradeLevel?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default)
        => _repo.GetWithSubjectsAsync(id, ct);

    public async Task<GradeLevel> CreateAsync(GradeLevel entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<GradeLevel> UpdateAsync(GradeLevel entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
