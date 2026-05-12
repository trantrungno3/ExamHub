using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho SchoolMember</summary>
public class SchoolMemberRepository : BaseRepository<SchoolMember, Guid>, ISchoolMemberRepository
{
    public SchoolMemberRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<SchoolMember>> GetBySchoolAsync(int schoolId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SchoolId == schoolId && x.IsActive)
            .OrderBy(x => x.Role).ThenBy(x => x.JoinedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SchoolMember>> GetBySchoolAndRoleAsync(int schoolId, string role, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SchoolId == schoolId && x.Role == role && x.IsActive)
            .OrderBy(x => x.JoinedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SchoolMember>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive)
            .Include(x => x.School)
            .ToListAsync(ct);

    public async Task<SchoolMember?> GetBySchoolAndUserAsync(int schoolId, Guid userId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.UserId == userId, ct);

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;
}
