using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Repository EF Core cho kỳ thi (exam sessions).</summary>
public class ExamSessionRepository(AppDbContext _db) : IExamSessionRepository
{
    // ── Quản lý ─────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public Task<ExamSession?> GetDetailAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ExamSession>()
            .Include(s => s.Subject)
            .Include(s => s.GradeLevel)
            .Include(s => s.Exams).ThenInclude(e => e.Exam)
            .Include(s => s.Assignments).ThenInclude(a => a.Cohort!).ThenInclude(c => c.School)
            .Include(s => s.Assignments).ThenInclude(a => a.CohortClass!).ThenInclude(cc => cc.Cohort!).ThenInclude(c => c.School)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<ExamSession> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? subjectId, int? gradeLevelId,
        ExamSessionStatusEnum? status, string? keyword, CancellationToken ct = default)
    {
        var query = _db.Set<ExamSession>()
            .Include(s => s.Subject)
            .Include(s => s.GradeLevel)
            .Include(s => s.Exams)
            .Include(s => s.Assignments)
            .AsQueryable();

        if (subjectId is not null) query = query.Where(s => s.SubjectId == subjectId.Value);
        if (gradeLevelId is not null) query = query.Where(s => s.GradeLevelId == gradeLevelId.Value);
        if (status is not null) query = query.Where(s => s.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = $"%{keyword.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.Title, kw));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    /// <inheritdoc/>
    public Task<ExamSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<ExamSession>().FirstOrDefaultAsync(s => s.Id == id, ct);

    /// <inheritdoc/>
    public async Task AddAsync(ExamSession s, CancellationToken ct = default)
    {
        _db.Set<ExamSession>().Add(s);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ExamSession s, CancellationToken ct = default)
    {
        _db.Set<ExamSession>().Update(s);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Set<ExamSession>().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return;
        _db.Set<ExamSession>().Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task SetStatusAsync(Guid id, ExamSessionStatusEnum status, CancellationToken ct = default)
    {
        var entity = await _db.Set<ExamSession>().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (entity is null) return;
        entity.Status = status;
        entity.Modified = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // ── Pool đề ─────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task AddExamsAsync(Guid sessionId, IEnumerable<Guid> examIds, CancellationToken ct = default)
    {
        var existing = await _db.Set<ExamSessionExam>()
            .Where(x => x.SessionId == sessionId).Select(x => x.ExamId).ToListAsync(ct);
        var toAdd = examIds.Distinct().Where(id => !existing.Contains(id))
            .Select(id => new ExamSessionExam { SessionId = sessionId, ExamId = id })
            .ToList();
        if (toAdd.Count == 0) return;
        _db.Set<ExamSessionExam>().AddRange(toAdd);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct = default)
    {
        var link = await _db.Set<ExamSessionExam>()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.ExamId == examId, ct);
        if (link is null) return;
        _db.Set<ExamSessionExam>().Remove(link);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Exam>> GetPoolExamsAsync(Guid sessionId, CancellationToken ct = default)
        => await _db.Set<ExamSessionExam>()
            .Where(x => x.SessionId == sessionId)
            .Select(x => x.Exam!)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public Task<bool> PoolContainsAsync(Guid sessionId, Guid examId, CancellationToken ct = default)
        => _db.Set<ExamSessionExam>().AnyAsync(x => x.SessionId == sessionId && x.ExamId == examId, ct);

    // ── Assignment ──────────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task AddAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default)
    {
        _db.Set<ExamSessionAssignment>().Add(a);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var entity = await _db.Set<ExamSessionAssignment>().FirstOrDefaultAsync(a => a.Id == assignmentId, ct);
        if (entity is null) return;
        _db.Set<ExamSessionAssignment>().Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<int> CountStudentsForAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default)
    {
        if (a.CohortClassId != null && a.CohortClass != null)
        {
            var cid = a.CohortClass.CohortId;
            var section = a.CohortClass.Section;
            return await _db.Set<CohortMember>()
                .CountAsync(m => m.IsActive && m.CohortId == cid && m.Section != null && m.Section == section, ct);
        }
        if (a.CohortId != null)
        {
            var cid = a.CohortId.Value;
            return await _db.Set<CohortMember>()
                .CountAsync(m => m.IsActive && m.CohortId == cid, ct);
        }
        return 0;
    }

    // ── Phía học sinh ───────────────────────────────────────────────────
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamSession>> GetAssignedToStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        var cohortIds = await _db.Set<CohortMember>()
            .Where(m => m.StudentId == studentId && m.IsActive)
            .Select(m => m.CohortId)
            .ToListAsync(ct);

        // Lớp cụ thể HS thuộc về: cùng cohort và section khớp
        var classIds = await _db.Set<CohortClass>()
            .Where(cc => _db.Set<CohortMember>().Any(m =>
                m.StudentId == studentId && m.IsActive &&
                m.CohortId == cc.CohortId && m.Section != null && m.Section == cc.Section))
            .Select(cc => cc.Id)
            .ToListAsync(ct);

        return await _db.Set<ExamSession>()
            .Include(s => s.Subject).Include(s => s.GradeLevel).Include(s => s.Assignments)
            .Where(s => s.Status == ExamSessionStatusEnum.Published)
            .Where(s => s.Assignments.Any(a =>
                (a.CohortId != null && cohortIds.Contains(a.CohortId.Value)) ||
                (a.CohortClassId != null && classIds.Contains(a.CohortClassId.Value))))
            .OrderByDescending(s => s.OpenAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> IsStudentAssignedAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
    {
        var cohortIds = await _db.Set<CohortMember>()
            .Where(m => m.StudentId == studentId && m.IsActive)
            .Select(m => m.CohortId)
            .ToListAsync(ct);

        var classIds = await _db.Set<CohortClass>()
            .Where(cc => _db.Set<CohortMember>().Any(m =>
                m.StudentId == studentId && m.IsActive &&
                m.CohortId == cc.CohortId && m.Section != null && m.Section == cc.Section))
            .Select(cc => cc.Id)
            .ToListAsync(ct);

        return await _db.Set<ExamSession>()
            .Where(s => s.Id == sessionId)
            .AnyAsync(s => s.Assignments.Any(a =>
                (a.CohortId != null && cohortIds.Contains(a.CohortId.Value)) ||
                (a.CohortClassId != null && classIds.Contains(a.CohortClassId.Value))), ct);
    }

    /// <inheritdoc/>
    public Task<int> CountSubmittedAttemptsAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => _db.Set<ExamSubmission>().CountAsync(
            x => x.SessionId == sessionId && x.StudentId == studentId
                 && (x.Status == SubmissionStatusEnum.Submitted || x.Status == SubmissionStatusEnum.Graded), ct);

    /// <inheritdoc/>
    public Task<ExamSubmission?> GetInProgressAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => _db.Set<ExamSubmission>().FirstOrDefaultAsync(
            x => x.SessionId == sessionId && x.StudentId == studentId
                 && x.Status == SubmissionStatusEnum.InProgress, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamSubmission>> GetStudentSubmissionsAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => await _db.Set<ExamSubmission>()
            .Where(x => x.SessionId == sessionId && x.StudentId == studentId)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task CreateSubmissionAsync(ExamSubmission submission, CancellationToken ct = default)
    {
        _db.Set<ExamSubmission>().Add(submission);
        await _db.SaveChangesAsync(ct);
    }
}
