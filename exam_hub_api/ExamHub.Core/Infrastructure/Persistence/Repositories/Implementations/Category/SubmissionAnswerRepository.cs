using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho SubmissionAnswer</summary>
public class SubmissionAnswerRepository : BaseRepository<SubmissionAnswer, Guid>, ISubmissionAnswerRepository
{
    /// <inheritdoc/>
    public SubmissionAnswerRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SubmissionAnswer>> GetBySubmissionAsync(
        Guid submissionId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SubmissionId == submissionId)
            .Include(x => x.ExamQuestion)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task DeleteBySubmissionAsync(Guid submissionId, CancellationToken ct = default)
        => await Set.Where(x => x.SubmissionId == submissionId).ExecuteDeleteAsync(ct);
}
