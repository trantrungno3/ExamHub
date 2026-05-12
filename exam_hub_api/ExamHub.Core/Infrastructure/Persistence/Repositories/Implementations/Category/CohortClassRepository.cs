using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho CohortClass</summary>
public class CohortClassRepository : BaseRepository<CohortClass, int>, ICohortClassRepository
{
    public CohortClassRepository(AppDbContext db) : base(db) { }

    public async Task<IReadOnlyList<CohortClass>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.CohortId == cohortId)
            .OrderBy(x => x.YearIndex)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CohortClass>> GetBySchoolYearAsync(string schoolYear, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SchoolYear == schoolYear)
            .OrderBy(x => x.ClassName)
            .ToListAsync(ct);

    public async Task<bool> SetHomeroomTeacherAsync(int id, Guid? teacherId, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.HomeroomTeacherId, teacherId), ct) > 0;
}
