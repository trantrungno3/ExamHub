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
}
