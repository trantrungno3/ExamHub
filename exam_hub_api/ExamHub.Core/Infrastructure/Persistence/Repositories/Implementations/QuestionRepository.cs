using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Caching;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using TVT.Core.Db.PostgreSql.Infrastructures;
using TVT.Core.Db.Redis;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho Question</summary>
public class QuestionRepository : BaseRepository<Question, Guid>, IQuestionRepository
{
    private readonly IRedisService _cache;
    private readonly IBaseRepository _sql;

    /// <inheritdoc/>
    public QuestionRepository(AppDbContext db, IRedisService cache, IBaseRepository sql) : base(db)
    {
        _cache = cache;
        _sql   = sql;
    }

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
            .OrderByDescending(x => x.Created)
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
        int? cognitiveLevelId = null,
        string? keyword = null,
        bool? isVerified = null,
        CancellationToken ct = default)
    {
        var query = Set.AsNoTracking()
            .Include(x => x.Topic)
            .Include(x => x.QuestionType)
            .Include(x => x.DifficultyLevel)
            .Include(x => x.CognitiveLevel)
            .AsQueryable();

        if (topicId.HasValue)
            query = query.Where(x => x.TopicId == topicId.Value);

        if (questionTypeId.HasValue)
            query = query.Where(x => x.QuestionTypeId == questionTypeId.Value);

        if (difficultyLevelId.HasValue)
            query = query.Where(x => x.DifficultyLevelId == difficultyLevelId.Value);

        if (cognitiveLevelId.HasValue)
            query = query.Where(x => x.CognitiveLevelId == cognitiveLevelId.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x =>
                EF.Functions.ILike(x.Content, $"%{keyword}%") ||
                (x.ContentPlain != null && EF.Functions.ILike(x.ContentPlain, $"%{keyword}%")));

        if (isVerified.HasValue)
            query = query.Where(x => x.IsVerified == isVerified.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PickedQuestion>> PickRandomAsync(
        int? topicId,
        int subjectId,
        int? questionTypeId,
        int difficultyId,
        int count,
        IReadOnlySet<Guid> excludeIds,
        int? cognitiveLevelId = null,
        CancellationToken ct = default)
    {
        if (count <= 0) return [];

        // 1. Pool ID ứng viên — cache 2 phút, chỉ lưu ID (spec §10).
        //    excludeIds thay đổi mỗi lần sinh đề nên KHÔNG nằm trong khóa cache.
        //    topicId null → pool toàn môn (mọi chủ đề của subjectId).
        var poolKey = topicId.HasValue
            ? QuestionPoolCache.PoolKey(topicId.Value, difficultyId, questionTypeId, cognitiveLevelId)
            : QuestionPoolCache.SubjectPoolKey(subjectId, difficultyId, questionTypeId, cognitiveLevelId);
        var pool = await _cache.GetOrSetAsync(
            poolKey,
            () => FetchPoolIdsAsync(topicId, subjectId, questionTypeId, difficultyId, cognitiveLevelId, ct),
            QuestionPoolCache.Ttl, ct);

        // 2. Loại trừ câu đã dùng + trộn Fisher-Yates + lấy count, tất cả trong bộ nhớ.
        var candidates = pool.Where(id => !excludeIds.Contains(id)).ToList();
        if (candidates.Count == 0) return [];
        Shuffle(candidates);
        var pickedIds = candidates.Take(count).ToList();

        // 3. Nạp nội dung + đáp án snapshot cho các ID đã chọn, giữ thứ tự ngẫu nhiên.
        var byId = await FetchByIdsAsync(pickedIds, ct);
        return pickedIds.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
    }

    private async Task<List<Guid>> FetchPoolIdsAsync(
        int? topicId, int subjectId, int? questionTypeId, int difficultyId, int? cognitiveLevelId, CancellationToken ct)
    {
        // topicId null → lọc theo môn (join topics) thay vì theo chủ đề.
        var sql = (topicId.HasValue, questionTypeId.HasValue, cognitiveLevelId.HasValue) switch
        {
            (true,  false, false) => PoolIdsSqlTopicNoTypeNoCog,
            (true,  true,  false) => PoolIdsSqlTopicWithTypeNoCog,
            (true,  false, true)  => PoolIdsSqlTopicNoTypeWithCog,
            (true,  true,  true)  => PoolIdsSqlTopicWithTypeWithCog,
            (false, false, false) => PoolIdsSqlSubjectNoTypeNoCog,
            (false, true,  false) => PoolIdsSqlSubjectWithTypeNoCog,
            (false, false, true)  => PoolIdsSqlSubjectNoTypeWithCog,
            (false, true,  true)  => PoolIdsSqlSubjectWithTypeWithCog,
        };
        var rows = await _sql.QueryAsync<Guid>(sql, new
        {
            TopicId          = topicId,
            SubjectId        = subjectId,
            QuestionTypeId   = questionTypeId,
            DifficultyId     = difficultyId,
            CognitiveLevelId = cognitiveLevelId
        }, cancellationToken: ct);
        return rows.ToList();
    }

    private async Task<Dictionary<Guid, PickedQuestion>> FetchByIdsAsync(
        IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return new Dictionary<Guid, PickedQuestion>();
        var rows = await _sql.QueryAsync<PickedQuestion>(FetchByIdsSql, new
        {
            Ids = ids.ToArray()
        }, cancellationToken: ct);
        return rows.ToDictionary(r => r.QuestionId);
    }

    private static void Shuffle(IList<Guid> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
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
                .SetProperty(x => x.RejectionReason, (string?)null)
                .SetProperty(x => x.VerifiedBy, verifiedBy)
                .SetProperty(x => x.VerifiedAt, DateTime.UtcNow), ct);

    /// <inheritdoc/>
    public async Task UnverifyAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsVerified, false)
                .SetProperty(x => x.RejectionReason, (string?)null)
                .SetProperty(x => x.VerifiedBy, (Guid?)null)
                .SetProperty(x => x.VerifiedAt, (DateTime?)null), ct);

    /// <inheritdoc/>
    public async Task RejectAsync(Guid id, Guid reviewedBy, string reason, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsVerified, false)
                .SetProperty(x => x.RejectionReason, reason)
                .SetProperty(x => x.VerifiedBy, reviewedBy)
                .SetProperty(x => x.VerifiedAt, DateTime.UtcNow), ct);

    /// <inheritdoc/>
    public async Task<QuestionStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        var total    = await Set.CountAsync(ct);
        var verified = await Set.CountAsync(x => x.IsVerified, ct);
        var rejected = await Set.CountAsync(x => !x.IsVerified && x.RejectionReason != null, ct);
        var pending  = await Set.CountAsync(x => !x.IsVerified && x.RejectionReason == null, ct);
        var inactive = await Set.CountAsync(x => !x.IsActive, ct);
        return new QuestionStatsResponse(total, verified, pending, rejected, inactive);
    }

    /// <inheritdoc/>
    public async Task SetImageUrlAsync(Guid id, string imageUrl, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ImageUrl, imageUrl)
                .SetProperty(x => x.Modified, DateTime.UtcNow), ct);

    /// <inheritdoc/>
    public async Task SetAudioUrlAsync(Guid id, string audioUrl, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.AudioUrl, audioUrl)
                .SetProperty(x => x.Modified, DateTime.UtcNow), ct);

    // ── SQL cho pool cache + nạp theo ID ──────────────────────────────────
    // Pool: chỉ SELECT id (cache được). 4 biến thể tránh truyền NULL int gây lỗi
    // type-inference trong Npgsql khi không lọc theo type/cognitive.

    private static readonly string PoolIdsSqlTopicNoTypeNoCog     = BuildPoolIdsSql(bySubject: false, filterByType: false, filterByCog: false);
    private static readonly string PoolIdsSqlTopicWithTypeNoCog   = BuildPoolIdsSql(bySubject: false, filterByType: true,  filterByCog: false);
    private static readonly string PoolIdsSqlTopicNoTypeWithCog   = BuildPoolIdsSql(bySubject: false, filterByType: false, filterByCog: true);
    private static readonly string PoolIdsSqlTopicWithTypeWithCog = BuildPoolIdsSql(bySubject: false, filterByType: true,  filterByCog: true);

    // Biến thể toàn môn (topicId null): join topics để lọc theo subject_id.
    private static readonly string PoolIdsSqlSubjectNoTypeNoCog     = BuildPoolIdsSql(bySubject: true, filterByType: false, filterByCog: false);
    private static readonly string PoolIdsSqlSubjectWithTypeNoCog   = BuildPoolIdsSql(bySubject: true, filterByType: true,  filterByCog: false);
    private static readonly string PoolIdsSqlSubjectNoTypeWithCog   = BuildPoolIdsSql(bySubject: true, filterByType: false, filterByCog: true);
    private static readonly string PoolIdsSqlSubjectWithTypeWithCog = BuildPoolIdsSql(bySubject: true, filterByType: true,  filterByCog: true);

    private static string BuildPoolIdsSql(bool bySubject, bool filterByType, bool filterByCog) => $"""
        SELECT q.id
        FROM public.questions q
        {(bySubject ? "JOIN public.topics t ON t.id = q.topic_id" : "")}
        WHERE q.is_active           = true
          AND q.is_verified         = true
          AND {(bySubject ? "t.subject_id = @SubjectId" : "q.topic_id = @TopicId")}
          AND q.difficulty_level_id = @DifficultyId
          {(filterByType ? "AND q.question_type_id   = @QuestionTypeId" : "")}
          {(filterByCog  ? "AND q.cognitive_level_id = @CognitiveLevelId" : "")}
        """;

    private const string FetchByIdsSql = """
        SELECT
            q.id      AS QuestionId,
            q.content AS Content,
            (
                SELECT jsonb_agg(
                    jsonb_build_object(
                        'id',          a.id,
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
        WHERE q.id = ANY(@Ids)
        """;
}
