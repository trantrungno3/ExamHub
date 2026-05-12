using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho CohortMember</summary>
public class CohortMemberService : ICohortMemberService
{
    private readonly ICohortMemberRepository _repo;
    public CohortMemberService(ICohortMemberRepository repo) => _repo = repo;

    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => _repo.GetByCohortAsync(cohortId, ct);

    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => _repo.GetByStudentAsync(studentId, ct);

    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<CohortMember> AddStudentAsync(CohortMember entity, CancellationToken ct = default)
    {
        entity.Id       = Guid.NewGuid();
        entity.JoinedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _repo.AddAsync(entity, ct);
    }

    public Task RemoveStudentAsync(Guid id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
