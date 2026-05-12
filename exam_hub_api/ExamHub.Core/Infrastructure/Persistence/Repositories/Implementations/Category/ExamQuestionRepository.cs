using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho ExamQuestion</summary>
public class ExamQuestionRepository : BaseRepository<ExamQuestion, Guid>, IExamQuestionRepository
{
    /// <inheritdoc/>
    public ExamQuestionRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamQuestion>> GetByExamAsync(Guid examId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.ExamId == examId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task DeleteByExamAsync(Guid examId, CancellationToken ct = default)
        => await Set.Where(x => x.ExamId == examId).ExecuteDeleteAsync(ct);
}
