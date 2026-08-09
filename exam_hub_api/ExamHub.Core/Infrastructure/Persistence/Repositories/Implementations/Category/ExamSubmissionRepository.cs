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
            .OrderByDescending(x => x.Created)
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
            .OrderByDescending(x => x.Created)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamSubmission>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .OrderByDescending(x => x.Created)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamSubmission>> GetBySessionAndStudentAsync(
        Guid sessionId, Guid studentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.SessionId == sessionId && x.StudentId == studentId)
            .OrderByDescending(x => x.Created)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<Guid, string>> GetStudentClassNamesAsync(
        IReadOnlyCollection<Guid> studentIds, CancellationToken ct = default)
    {
        var result = new Dictionary<Guid, string>();
        if (studentIds.Count == 0) return result;

        // Membership đang hoạt động của các HS: studentId → (cohortId, section).
        var members = await Db.Set<CohortMember>().AsNoTracking()
            .Where(m => m.IsActive && m.Section != null && studentIds.Contains(m.StudentId))
            .Select(m => new { m.StudentId, m.CohortId, m.Section })
            .ToListAsync(ct);
        if (members.Count == 0) return result;

        // Các lớp thuộc những cohort liên quan; khớp theo (cohortId, section).
        var cohortIds = members.Select(m => m.CohortId).Distinct().ToList();
        var classes = await Db.Set<CohortClass>().AsNoTracking()
            .Where(cc => cohortIds.Contains(cc.CohortId))
            .Select(cc => new { cc.CohortId, cc.Section, cc.ClassName, cc.SchoolYear })
            .ToListAsync(ct);

        foreach (var m in members)
        {
            if (result.ContainsKey(m.StudentId)) continue;
            // HS có thể thuộc nhiều năm học → lấy lớp có năm học mới nhất.
            var cc = classes
                .Where(c => c.CohortId == m.CohortId && c.Section == m.Section)
                .OrderByDescending(c => c.SchoolYear)
                .FirstOrDefault();
            if (cc != null) result[m.StudentId] = cc.ClassName;
        }
        return result;
    }
}
