using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho Question</summary>
public class QuestionRepository : BaseRepository<Question, Guid>, IQuestionRepository
{
    /// <inheritdoc/>
    public QuestionRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<Question?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Include(x => x.Answers.OrderBy(a => a.SortOrder))
            .Include(x => x.Topic)
            .Include(x => x.QuestionType)
            .Include(x => x.DifficultyLevel)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Question>> GetByTopicAsync(
        int topicId, bool activeOnly = true, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.TopicId == topicId && (!activeOnly || x.IsActive))
            .Include(x => x.Answers.OrderBy(a => a.SortOrder))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Question>> GetPoolAsync(
        int? topicId,
        int? questionTypeId,
        int? difficultyLevelId,
        IEnumerable<Guid>? excludeIds = null,
        CancellationToken ct = default)
    {
        // Build IQueryable<Question> with all filters first, then Include at the end
        IQueryable<Question> query = Set.AsNoTracking()
            .Where(x => x.IsActive && x.IsVerified);

        if (topicId.HasValue)
            query = query.Where(x => x.TopicId == topicId.Value);

        if (questionTypeId.HasValue)
            query = query.Where(x => x.QuestionTypeId == questionTypeId.Value);

        if (difficultyLevelId.HasValue)
            query = query.Where(x => x.DifficultyLevelId == difficultyLevelId.Value);

        if (excludeIds is not null)
        {
            var excludeList = excludeIds.ToList();
            if (excludeList.Count > 0)
                query = query.Where(x => !excludeList.Contains(x.Id));
        }

        return await query.Include(x => x.Answers).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Question> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        int? topicId = null,
        int? questionTypeId = null,
        int? difficultyLevelId = null,
        string? keyword = null,
        bool? isVerified = null,
        CancellationToken ct = default)
    {
        var query = Set.AsNoTracking()
            .Include(x => x.Topic)
            .Include(x => x.QuestionType)
            .Include(x => x.DifficultyLevel)
            .AsQueryable();

        if (topicId.HasValue)
            query = query.Where(x => x.TopicId == topicId.Value);

        if (questionTypeId.HasValue)
            query = query.Where(x => x.QuestionTypeId == questionTypeId.Value);

        if (difficultyLevelId.HasValue)
            query = query.Where(x => x.DifficultyLevelId == difficultyLevelId.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.ContentPlain != null && x.ContentPlain.ToLower().Contains(keyword.ToLower()));

        if (isVerified.HasValue)
            query = query.Where(x => x.IsVerified == isVerified.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    /// <inheritdoc/>
    public async Task IncrementUsageCountAsync(IEnumerable<Guid> questionIds, CancellationToken ct = default)
    {
        var ids = questionIds.ToList();
        await Set
            .Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UsageCount, x => x.UsageCount + 1), ct);
    }

    /// <inheritdoc/>
    public async Task VerifyAsync(Guid id, Guid verifiedBy, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsVerified, true)
                .SetProperty(x => x.VerifiedBy, verifiedBy)
                .SetProperty(x => x.VerifiedAt, DateTime.UtcNow), ct);
}

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

/// <summary>Triển khai repository cho TeacherSubject</summary>
public class TeacherSubjectRepository : BaseRepository<TeacherSubject, int>, ITeacherSubjectRepository
{
    /// <inheritdoc/>
    public TeacherSubjectRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Subject)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => await Set.AnyAsync(x => x.UserId == userId && x.SubjectId == subjectId, ct);

    /// <inheritdoc/>
    public async Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
    {
        var exists = await IsTeacherOfSubjectAsync(userId, subjectId, ct);
        if (!exists)
        {
            await Set.AddAsync(new TeacherSubject { UserId = userId, SubjectId = subjectId }, ct);
            await Db.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => await Set
            .Where(x => x.UserId == userId && x.SubjectId == subjectId)
            .ExecuteDeleteAsync(ct);
}

