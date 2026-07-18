using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho ExamTemplate</summary>
public class ExamTemplateRepository : BaseRepository<ExamTemplate, Guid>, IExamTemplateRepository
{
    /// <inheritdoc/>
    public ExamTemplateRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<ExamTemplate?> GetWithSectionsAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Sections.OrderBy(s => s.SortOrder))
            .Include(x => x.GradeLevel)
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamTemplate>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SubjectId == subjectId && x.IsActive)
            .OrderByDescending(x => x.Created)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamTemplate>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.GradeLevelId == gradeLevelId && x.IsActive)
            .OrderByDescending(x => x.Created)
            .ToListAsync(ct);
}
