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
    public override async Task<IReadOnlyList<ExamTemplate>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(x => x.GradeLevel)
            .Include(x => x.Subject)
            .OrderByDescending(x => x.Created)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamTemplate>> GetFilteredAsync(int? subjectId, int? gradeLevelId, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking()
            .Include(x => x.GradeLevel)
            .Include(x => x.Subject)
            .AsQueryable();
        if (subjectId is not null) query = query.Where(x => x.SubjectId == subjectId);
        if (gradeLevelId is not null) query = query.Where(x => x.GradeLevelId == gradeLevelId);
        return await query.OrderByDescending(x => x.Created).ToListAsync(ct);
    }
}
