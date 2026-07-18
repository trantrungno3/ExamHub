using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho School</summary>
public class SchoolService : ISchoolService
{
    private readonly ISchoolRepository _repo;
    public SchoolService(ISchoolRepository repo) => _repo = repo;

    public Task<IReadOnlyList<School>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<IReadOnlyList<School>> GetActiveAsync(CancellationToken ct = default)
        => _repo.GetActiveAsync(ct);

    public Task<School?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => _repo.ExistsAsync(e => e.Id == id, ct);

    public Task<School?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _repo.GetByCodeAsync(code, ct);

    public Task<School?> GetWithCohortsAsync(int id, CancellationToken ct = default)
        => _repo.GetWithCohortsAsync(id, ct);

    public Task<School?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => _repo.GetWithMembersAsync(id, ct);

    public async Task<School> CreateAsync(School entity, CancellationToken ct = default)
    {
        entity.Created = DateTime.UtcNow;
        entity.Modified = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<School> UpdateAsync(School entity, CancellationToken ct = default)
    {
        entity.Modified = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
