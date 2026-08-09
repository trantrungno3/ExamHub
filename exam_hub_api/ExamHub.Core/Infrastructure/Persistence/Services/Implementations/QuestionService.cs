using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Caching;
using TVT.Core.Db.Redis;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Question</summary>
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IQuestionAnswerRepository _answerRepo;
    private readonly ITopicRepository _topicRepo;
    private readonly IRedisService _cache;

    public QuestionService(IQuestionRepository questionRepo, IQuestionAnswerRepository answerRepo, ITopicRepository topicRepo, IRedisService cache)
    {
        _questionRepo = questionRepo;
        _answerRepo   = answerRepo;
        _topicRepo    = topicRepo;
        _cache        = cache;
    }

    public Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _questionRepo.GetByIdAsync(id, ct);

    public Task<Question?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => _questionRepo.GetWithAnswersAsync(id, ct);

    public Task<IReadOnlyList<Question>> GetByTopicAsync(int topicId, CancellationToken ct = default)
        => _questionRepo.GetByTopicAsync(topicId, ct: ct);

    public Task<(IReadOnlyList<Question> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        int? topicId = null, int? questionTypeId = null,
        int? difficultyLevelId = null, int? cognitiveLevelId = null,
        string? keyword = null,
        bool? isVerified = null, string? reviewStatus = null, CancellationToken ct = default)
        => _questionRepo.GetPagedAsync(page, pageSize, topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, keyword, isVerified, reviewStatus, ct);

    public async Task<Question> CreateAsync(Question entity, IEnumerable<QuestionAnswer> answers, CancellationToken ct = default)
    {
        entity.Id        = Guid.NewGuid();
        entity.Created = DateTime.UtcNow;
        entity.Modified = DateTime.UtcNow;

        await _questionRepo.AddAsync(entity, ct);

        var answerList = answers.Select((a, i) =>
        {
            a.Id         = Guid.NewGuid();
            a.QuestionId = entity.Id;
            a.SortOrder  = (short)i;
            return a;
        }).ToList();

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);

        await InvalidatePoolAsync(entity, ct);
        return entity;
    }

    public async Task<Question> UpdateAsync(Question entity, IEnumerable<QuestionAnswer>? answers = null, CancellationToken ct = default)
    {
        // Lấy phân loại cũ trước khi cập nhật để invalidate cả pool cũ (trường hợp đổi topic/độ khó/loại/Bloom).
        var old = await _questionRepo.GetByIdAsync(entity.Id, ct);

        entity.Modified = DateTime.UtcNow;
        await _questionRepo.UpdateAsync(entity, ct);

        if (answers is not null)
        {
            await _answerRepo.DeleteByQuestionAsync(entity.Id, ct);
            var answerList = answers.Select((a, i) =>
            {
                a.Id         = Guid.NewGuid();
                a.QuestionId = entity.Id;
                a.SortOrder  = (short)i;
                return a;
            }).ToList();
            if (answerList.Count > 0)
                await _answerRepo.AddRangeAsync(answerList, ct);
        }

        if (old is not null) await InvalidatePoolAsync(old, ct);
        await InvalidatePoolAsync(entity, ct);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _questionRepo.GetByIdAsync(id, ct);
        await _questionRepo.DeleteByIdAsync(id, ct);
        if (existing is not null) await InvalidatePoolAsync(existing, ct);
    }

    public async Task VerifyAsync(Guid id, Guid verifiedBy, CancellationToken ct = default)
    {
        await _questionRepo.VerifyAsync(id, verifiedBy, ct);
        // Duyệt câu hỏi đưa nó vào pool (pool chỉ gồm is_verified=true) → invalidate để refetch.
        var verified = await _questionRepo.GetByIdAsync(id, ct);
        if (verified is not null) await InvalidatePoolAsync(verified, ct);
    }

    public async Task UnverifyAsync(Guid id, CancellationToken ct = default)
    {
        // Bỏ duyệt loại câu hỏi khỏi pool (pool chỉ gồm is_verified=true) → invalidate trước khi đổi.
        var question = await _questionRepo.GetByIdAsync(id, ct);
        if (question is not null) await InvalidatePoolAsync(question, ct);
        await _questionRepo.UnverifyAsync(id, ct);
    }

    public async Task RejectAsync(Guid id, Guid reviewedBy, string reason, CancellationToken ct = default)
    {
        // Từ chối cũng loại câu hỏi khỏi pool → invalidate trước khi đổi.
        var question = await _questionRepo.GetByIdAsync(id, ct);
        if (question is not null) await InvalidatePoolAsync(question, ct);
        await _questionRepo.RejectAsync(id, reviewedBy, reason, ct);
    }

    public Task<QuestionStatsResponse> GetStatsAsync(CancellationToken ct = default)
        => _questionRepo.GetStatsAsync(ct);

    public Task SetImageUrlAsync(Guid id, string imageUrl, CancellationToken ct = default)
        => _questionRepo.SetImageUrlAsync(id, imageUrl, ct);

    public Task SetAudioUrlAsync(Guid id, string audioUrl, CancellationToken ct = default)
        => _questionRepo.SetAudioUrlAsync(id, audioUrl, ct);

    /// <summary>Xóa các khóa pool Redis mà câu hỏi này tham gia (pool theo chủ đề + pool toàn môn, ≤ 8 khóa).</summary>
    private async Task InvalidatePoolAsync(Question q, CancellationToken ct)
    {
        // Cần subjectId để invalidate pool toàn môn (sinh đề không chọn chủ đề).
        var topic = await _topicRepo.GetByIdAsync(q.TopicId, ct);
        var subjectId = topic?.SubjectId ?? 0;
        var removals = QuestionPoolCache
            .KeysForQuestion(q.TopicId, subjectId, q.DifficultyLevelId, q.QuestionTypeId, q.CognitiveLevelId)
            .Select(key => _cache.RemoveAsync(key, ct));
        await Task.WhenAll(removals);
    }
}
