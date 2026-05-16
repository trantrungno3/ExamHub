using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho CognitiveLevel</summary>
public class CognitiveLevelRepository : CategoryRepository<CognitiveLevel, int>, ICognitiveLevelRepository
{
    /// <inheritdoc/>
    public CognitiveLevelRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<CognitiveLevel>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.LevelOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<CognitiveLevel>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => EF.Functions.ILike(x.Name, $"%{keyword}%")
                     || EF.Functions.ILike(x.NameEn, $"%{keyword}%"))
            .OrderBy(x => x.LevelOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<CognitiveLevel?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Set.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);
}
