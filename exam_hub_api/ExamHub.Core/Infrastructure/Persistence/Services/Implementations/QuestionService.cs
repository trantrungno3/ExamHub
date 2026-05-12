using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Question</summary>
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IQuestionAnswerRepository _answerRepo;

    public QuestionService(IQuestionRepository questionRepo, IQuestionAnswerRepository answerRepo)
    {
        _questionRepo = questionRepo;
        _answerRepo   = answerRepo;
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
        int? difficultyLevelId = null, string? keyword = null,
        bool? isVerified = null, CancellationToken ct = default)
        => _questionRepo.GetPagedAsync(page, pageSize, topicId, questionTypeId, difficultyLevelId, keyword, isVerified, ct);

    public async Task<Question> CreateAsync(Question entity, IEnumerable<QuestionAnswer> answers, CancellationToken ct = default)
    {
        entity.Id        = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

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

        return entity;
    }

    public async Task<Question> UpdateAsync(Question entity, IEnumerable<QuestionAnswer>? answers = null, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
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

        return entity;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _questionRepo.DeleteByIdAsync(id, ct);

    public Task VerifyAsync(Guid id, Guid verifiedBy, CancellationToken ct = default)
        => _questionRepo.VerifyAsync(id, verifiedBy, ct);
}
