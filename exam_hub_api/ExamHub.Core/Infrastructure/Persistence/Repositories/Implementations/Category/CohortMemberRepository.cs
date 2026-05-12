using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho CohortMember</summary>
public class CohortMemberRepository : BaseRepository<CohortMember, Guid>, ICohortMemberRepository
{
    public CohortMemberRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.CohortId == cohortId && x.IsActive)
            .OrderBy(x => x.JoinedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Include(x => x.Cohort)
            .ToListAsync(ct);

    public async Task<CohortMember?> GetByCohortAndStudentAsync(int cohortId, Guid studentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CohortId == cohortId && x.StudentId == studentId, ct);

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;
}
