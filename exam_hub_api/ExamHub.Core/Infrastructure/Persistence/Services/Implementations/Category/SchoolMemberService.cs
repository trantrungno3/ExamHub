using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho SchoolMember</summary>
public class SchoolMemberService : ISchoolMemberService
{
    private readonly ISchoolMemberRepository _repo;
    public SchoolMemberService(ISchoolMemberRepository repo) => _repo = repo;

    public Task<IReadOnlyList<SchoolMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default)
        => _repo.GetBySchoolAsync(schoolId, ct);

    public Task<IReadOnlyList<SchoolMember>> GetBySchoolAndRoleAsync(int schoolId, string role, CancellationToken ct = default)
        => _repo.GetBySchoolAndRoleAsync(schoolId, role, ct);

    public Task<IReadOnlyList<SchoolMember>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => _repo.GetByUserAsync(userId, ct);

    public Task<SchoolMember?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<SchoolMember> AddMemberAsync(SchoolMember entity, CancellationToken ct = default)
    {
        entity.Id       = Guid.NewGuid();
        entity.JoinedAt = DateTimeOffset.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public async Task<SchoolMember> UpdateAsync(SchoolMember entity, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(entity, ct);
        return entity;
    }

    public Task RemoveMemberAsync(Guid id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);
}
