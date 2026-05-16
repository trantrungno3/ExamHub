using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho Subject</summary>
public class SubjectRepository : CategoryRepository<Subject, int>, ISubjectRepository
{
    /// <inheritdoc/>
    public SubjectRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Subject>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Subject>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => EF.Functions.ILike(x.Name, $"%{keyword}%"))
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Subject>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.GradeLevelId == gradeLevelId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<Subject?> GetWithTopicsAsync(int id, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(x => x.Topics)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}
