using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho ExamTemplateSection</summary>
public class ExamTemplateSectionRepository
    : BaseRepository<ExamTemplateSection, Guid>, IExamTemplateSectionRepository
{
    /// <inheritdoc/>
    public ExamTemplateSectionRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamTemplateSection>> GetByTemplateAsync(
        Guid templateId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.ExamTemplateId == templateId)
            .Include(x => x.Topic)
            .Include(x => x.QuestionType)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task DeleteByTemplateAsync(Guid templateId, CancellationToken ct = default)
        => await Set.Where(x => x.ExamTemplateId == templateId).ExecuteDeleteAsync(ct);
}
