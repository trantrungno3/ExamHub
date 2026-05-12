using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho QuestionAnswer</summary>
public class QuestionAnswerRepository : BaseRepository<QuestionAnswer, Guid>, IQuestionAnswerRepository
{
    /// <inheritdoc/>
    public QuestionAnswerRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<QuestionAnswer>> GetByQuestionAsync(Guid questionId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.QuestionId == questionId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task DeleteByQuestionAsync(Guid questionId, CancellationToken ct = default)
        => await Set.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync(ct);
}
