using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho Cohort</summary>
public class CohortRepository : BaseRepository<Cohort, int>, ICohortRepository
{
    public CohortRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Cohort>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.StartYear)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Cohort>> GetBySchoolAsync(int schoolId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SchoolId == schoolId)
            .OrderByDescending(x => x.StartYear)
            .ToListAsync(ct);

    public async Task<Cohort?> GetWithClassesAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Classes.OrderBy(c => c.YearIndex))
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Cohort?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Members.Where(m => m.IsActive))
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;
}
