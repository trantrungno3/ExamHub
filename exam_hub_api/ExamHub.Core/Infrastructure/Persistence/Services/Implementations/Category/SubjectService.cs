using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Subject</summary>
public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;
    public SubjectService(ISubjectRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Subject>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<Subject>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => _repo.GetByGradeLevelAsync(gradeLevelId, ct);

    public Task<Subject?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default)
        => _repo.GetWithTopicsAsync(id, ct);

    public async Task<Subject> CreateAsync(Subject entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<Subject> UpdateAsync(Subject entity, CancellationToken ct = default)
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
