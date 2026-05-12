using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho GradeLevel</summary>
public class GradeLevelRepository : CategoryRepository<GradeLevel, int>, IGradeLevelRepository
{
    /// <inheritdoc/>
    public GradeLevelRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<GradeLevel>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.GradeNumber)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<GradeLevel>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(x => x.GradeNumber)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<GradeLevel?> GetWithSubjectsAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Subjects)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}
