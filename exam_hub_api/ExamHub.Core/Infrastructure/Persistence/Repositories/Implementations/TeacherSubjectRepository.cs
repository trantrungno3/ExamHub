using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho TeacherSubject</summary>
public class TeacherSubjectRepository : BaseRepository<TeacherSubject, int>, ITeacherSubjectRepository
{
    /// <inheritdoc/>
    public TeacherSubjectRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => await Set.AnyAsync(x => x.UserId == userId && x.SubjectId == subjectId, ct);

    /// <inheritdoc/>
    public async Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
    {
        var exists = await IsTeacherOfSubjectAsync(userId, subjectId, ct);
        if (!exists)
        {
            await Set.AddAsync(new TeacherSubject { UserId = userId, SubjectId = subjectId }, ct);
            await Db.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => await Set
            .Where(x => x.UserId == userId && x.SubjectId == subjectId)
            .ExecuteDeleteAsync(ct);

    /// <inheritdoc/>
    public async Task SetSubjectsAsync(Guid userId, IReadOnlyList<int> subjectIds, CancellationToken ct = default)
    {
        var wanted = subjectIds.Distinct().ToList();
        var existingIds = await Set.Where(x => x.UserId == userId)
            .Select(x => x.SubjectId).ToListAsync(ct);

        var toRemove = existingIds.Except(wanted).ToList();
        var toAdd = wanted.Except(existingIds).ToList();

        if (toRemove.Count > 0)
            await Set.Where(x => x.UserId == userId && toRemove.Contains(x.SubjectId))
                .ExecuteDeleteAsync(ct);

        if (toAdd.Count > 0)
            await AddRangeAsync(toAdd.Select(id => new TeacherSubject { UserId = userId, SubjectId = id }), ct);
    }
}
