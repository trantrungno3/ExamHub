using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
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
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExamTemplate>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.GradeLevelId == gradeLevelId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
}

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
                x.Title.ToLower().Contains(keyword.ToLower()) ||
                (x.ExamCode != null && x.ExamCode.ToLower().Contains(keyword.ToLower())));

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

