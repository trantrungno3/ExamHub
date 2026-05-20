using Dapper;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho Question</summary>
public class QuestionRepository : BaseRepository<Question, Guid>, IQuestionRepository
{
    /// <inheritdoc/>
    public QuestionRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<Question?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(x => x.Answers.OrderBy(a => a.SortOrder))
            .Include(x => x.Topic)
            .Include(x => x.QuestionType)
            .Include(x => x.DifficultyLevel)
            .Include(x => x.CognitiveLevel)
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
            query = query.Where(x =>
                EF.Functions.ILike(x.Content, $"%{keyword}%") ||
                (x.ContentPlain != null && EF.Functions.ILike(x.ContentPlain, $"%{keyword}%")));

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
    public async Task<IReadOnlyList<PickedQuestion>> PickRandomAsync(
        int topicId,
        int? questionTypeId,
        int difficultyId,
        int count,
        IReadOnlySet<Guid> excludeIds,
        int? cognitiveLevelId = null,
        CancellationToken ct = default)
    {
        var sql = (questionTypeId.HasValue, cognitiveLevelId.HasValue) switch
        {
            (false, false) => PickSqlNoTypeNoCog,
            (true,  false) => PickSqlWithTypeNoCog,
            (false, true)  => PickSqlNoTypeWithCog,
            (true,  true)  => PickSqlWithTypeWithCog,
        };
        await using var conn = new NpgsqlConnection(Db.Database.GetConnectionString());
        var rows = await conn.QueryAsync<PickedQuestion>(sql, new
        {
            TopicId          = topicId,
            QuestionTypeId   = questionTypeId,
            DifficultyId     = difficultyId,
            CognitiveLevelId = cognitiveLevelId,
            ExcludedIds      = excludeIds.Count > 0 ? excludeIds.ToArray() : Array.Empty<Guid>(),
            Count            = count
        });
        return rows.ToList();
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

    // ── SQL cho PickRandomAsync ───────────────────────────────────────────
    // 4 biến thể tránh truyền NULL int gây lỗi type-inference trong Npgsql.

    private static readonly string PickSqlNoTypeNoCog    = BuildPickSql(filterByType: false, filterByCog: false);
    private static readonly string PickSqlWithTypeNoCog  = BuildPickSql(filterByType: true,  filterByCog: false);
    private static readonly string PickSqlNoTypeWithCog  = BuildPickSql(filterByType: false, filterByCog: true);
    private static readonly string PickSqlWithTypeWithCog = BuildPickSql(filterByType: true, filterByCog: true);

    private static string BuildPickSql(bool filterByType, bool filterByCog) => $"""
        SELECT
            q.id      AS QuestionId,
            q.content AS Content,
            (
                SELECT jsonb_agg(
                    jsonb_build_object(
                        'content',     a.content,
                        'is_correct',  a.is_correct,
                        'sort_order',  a.sort_order,
                        'explanation', a.explanation
                    ) ORDER BY a.sort_order
                )::text
                FROM public.question_answers a
                WHERE a.question_id = q.id
            ) AS AnswersJson
        FROM public.questions q
        WHERE q.is_active           = true
          AND q.is_verified         = true
          AND q.topic_id            = @TopicId
          AND q.difficulty_level_id = @DifficultyId
          {(filterByType ? "AND q.question_type_id   = @QuestionTypeId" : "")}
          {(filterByCog  ? "AND q.cognitive_level_id = @CognitiveLevelId" : "")}
          AND NOT (q.id = ANY(@ExcludedIds))
        ORDER BY RANDOM()
        LIMIT @Count
        """;
}
