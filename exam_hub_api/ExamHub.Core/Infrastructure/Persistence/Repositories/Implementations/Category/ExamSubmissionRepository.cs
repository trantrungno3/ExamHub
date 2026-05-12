using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho ExamSubmission</summary>
public class ExamSubmissionRepository : BaseRepository<ExamSubmission, Guid>, IExamSubmissionRepository
{
    /// <inheritdoc/>
    public ExamSubmissionRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<ExamSubmission?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.ExamId == examId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<ExamSubmission?> GetByExamAndStudentAsync(
        Guid examId, Guid studentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExamId == examId && x.StudentId == studentId, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Include(x => x.Exam)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
}
