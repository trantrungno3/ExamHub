using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho School</summary>
public class SchoolRepository : BaseRepository<School, int>, ISchoolRepository
{
    public SchoolRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<School>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<School?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<School?> GetWithCohortsAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Cohorts)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<School?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;
}
