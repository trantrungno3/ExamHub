using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho Topic</summary>
public class TopicRepository : CategoryRepository<Topic, int>, ITopicRepository
{
    /// <inheritdoc/>
    public TopicRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Topic>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<Topic>> SearchByNameAsync(string keyword, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public override async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, isActive), ct) > 0;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Topic>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SubjectId == subjectId && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Topic>> GetChildrenAsync(int parentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.ParentId == parentId && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Topic>> GetRootTopicsAsync(int subjectId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SubjectId == subjectId && x.ParentId == null && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);
}
