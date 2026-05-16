using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho Exam</summary>
public class ExamRepository : BaseRepository<Exam, Guid>, IExamRepository
{
    /// <inheritdoc/>
    public ExamRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Questions.OrderBy(q => q.SortOrder))
            .Include(x => x.GradeLevel)
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Exam>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SubjectId == subjectId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Exam>> GetByStatusAsync(ExamStatusEnum status, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.ParentExamId == parentExamId)
            .OrderBy(x => x.VariantIndex)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        int? gradeLevelId = null,
        int? subjectId = null,
        ExamStatusEnum? status = null,
        string? keyword = null,
        CancellationToken ct = default)
    {
        var query = Set.AsNoTracking()
            .Include(x => x.GradeLevel)
            .Include(x => x.Subject)
            .AsQueryable();

        if (gradeLevelId.HasValue)
            query = query.Where(x => x.GradeLevelId == gradeLevelId.Value);

        if (subjectId.HasValue)
            query = query.Where(x => x.SubjectId == subjectId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{keyword}%") ||
                (x.ExamCode != null && EF.Functions.ILike(x.ExamCode, $"%{keyword}%")));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateStatusAsync(Guid id, ExamStatusEnum status, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, status)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct) > 0;
}
