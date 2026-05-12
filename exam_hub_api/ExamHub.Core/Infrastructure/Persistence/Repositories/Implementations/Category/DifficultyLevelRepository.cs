using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho DifficultyLevel</summary>
public class DifficultyLevelRepository : CategoryRepository<DifficultyLevel, int>, IDifficultyLevelRepository
{
    /// <inheritdoc/>
    public DifficultyLevelRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<DifficultyLevel>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<DifficultyLevel>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<DifficultyLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);
}
