using System.Linq.Expressions;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Xunit;

namespace ExamHub.Tests;

// ── Hand-rolled fakes (no mocking library in this repo — xunit only) ──────

/// <summary>Fake IQuestionRepository backed by a fixed in-memory pool of IDs.</summary>
file sealed class FakeQuestionPoolRepository(List<Guid> pool) : IQuestionRepository
{
    public Task<IReadOnlyList<PickedQuestion>> PickRandomAsync(
        int? topicId, int subjectId, int? questionTypeId, int difficultyId, int count,
        IReadOnlySet<Guid> excludeIds, int? cognitiveLevelId = null, CancellationToken ct = default)
    {
        var picked = pool.Where(id => !excludeIds.Contains(id)).Take(count)
            .Select(id => new PickedQuestion(id, $"content-{id:N}", null))
            .ToList();
        return Task.FromResult<IReadOnlyList<PickedQuestion>>(picked);
    }

    public Task<Question?> GetWithAnswersAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Question>> GetByTopicAsync(int topicId, bool activeOnly = true, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Question>> GetPoolAsync(int? topicId, int? questionTypeId, int? difficultyLevelId, IEnumerable<Guid>? excludeIds = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<(IReadOnlyList<Question> Items, int Total)> GetPagedAsync(int page, int pageSize, int? topicId = null, int? questionTypeId = null, int? difficultyLevelId = null, int? cognitiveLevelId = null, string? keyword = null, string? reviewStatus = null, int? subjectId = null, int? gradeLevelId = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task IncrementUsageCountAsync(IEnumerable<Guid> questionIds, CancellationToken ct = default) => throw new NotSupportedException();
    public Task VerifyAsync(Guid id, Guid verifiedBy, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UnverifyAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task RejectAsync(Guid id, Guid reviewedBy, string reason, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<QuestionStatsResponse> GetStatsAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task SetImageUrlAsync(Guid id, string imageUrl, CancellationToken ct = default) => throw new NotSupportedException();
    public Task SetAudioUrlAsync(Guid id, string audioUrl, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Question>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Question>> GetAsync(Expression<Func<Question, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Question?> FirstOrDefaultAsync(Expression<Func<Question, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<Question, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<Question, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Question> AddAsync(Question entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<Question> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(Question entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(Question entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

/// <summary>Fake IExamGeneratorRepository that just captures what would have been saved.</summary>
file sealed class FakeExamGeneratorRepository : IExamGeneratorRepository
{
    public List<Exam> SavedExams { get; } = [];
    public List<ExamQuestion> SavedQuestions { get; } = [];

    public Task<Guid> SaveExamAsync(Exam exam, IReadOnlyList<ExamQuestion> questions, IReadOnlySet<Guid> usedQuestionIds, CancellationToken ct = default)
    {
        SavedExams.Add(exam);
        SavedQuestions.AddRange(questions);
        return Task.FromResult(exam.Id);
    }

    public Task<Guid> SaveBatchExamsAsync(IReadOnlyList<Exam> exams, IReadOnlyList<ExamQuestion> questions, IReadOnlySet<Guid> usedQuestionIds, CancellationToken ct = default)
    {
        SavedExams.AddRange(exams);
        SavedQuestions.AddRange(questions);
        return Task.FromResult(exams[0].BatchId ?? Guid.NewGuid());
    }
}

/// <summary>Fake IExamTemplateRepository — never actually called (tests always pass ExamTemplateId=null).</summary>
file sealed class FakeExamTemplateRepository : IExamTemplateRepository
{
    public Task<ExamTemplate?> GetWithSectionsAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamTemplate>> GetBySubjectAsync(int subjectId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamTemplate>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<ExamTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamTemplate>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamTemplate>> GetAsync(Expression<Func<ExamTemplate, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<ExamTemplate?> FirstOrDefaultAsync(Expression<Func<ExamTemplate, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<ExamTemplate, bool>> predicate, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<ExamTemplate, bool>>? predicate = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<ExamTemplate> AddAsync(ExamTemplate entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<ExamTemplate> entities, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UpdateAsync(ExamTemplate entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(ExamTemplate entity, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotSupportedException();
}

public class ExamGeneratorDuplicateTests
{
    // 1 section, 100% Easy, N câu — đơn giản hoá difficulty split để mọi câu đến từ 1 lần PickRandomAsync.
    private static SectionConfig EasySection(int count) => new()
    {
        QuestionCount = count, PctEasy = 100, PctMedium = 0, PctHard = 0, PctVeryHard = 0,
        ScorePerQuestion = 1
    };

    [Fact]
    public async Task BatchGenerateAsync_PreventDuplicateTrue_NoQuestionRepeatsAcrossVariants()
    {
        var pool = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList(); // đúng bằng 3 variant x 2 câu
        var examRepo = new FakeExamGeneratorRepository();
        var service = new ExamGeneratorService(examRepo, new FakeQuestionPoolRepository(pool), new FakeExamTemplateRepository());

        // Title, ExamTemplateId, GradeLevelId, SubjectId, DurationMinutes, ShuffleQuestions, ShuffleAnswers,
        // PreventDuplicate, TotalScore, VariantCount, VariantNaming, CreatedBy, Sections
        var request = new BatchGenerateExamRequest(
            "Đề kiểm tra", null, 1, 1, 45, false, false, true, 0,
            3, "ALPHA", "gv1", [EasySection(2)]);

        await service.BatchGenerateAsync(request);

        Assert.Equal(6, examRepo.SavedQuestions.Count);
        Assert.Equal(6, examRepo.SavedQuestions.Select(q => q.QuestionId).Distinct().Count());
        Assert.Equal(3, examRepo.SavedExams.Select(e => e.Title).Distinct().Count()); // mỗi variant 1 title riêng
    }

    [Fact]
    public async Task BatchGenerateAsync_PreventDuplicateFalse_AllVariantsShareSameQuestionSet()
    {
        var pool = Enumerable.Range(0, 2).Select(_ => Guid.NewGuid()).ToList(); // chỉ đủ cho 1 lượt pick, không đủ cho 3
        var examRepo = new FakeExamGeneratorRepository();
        var service = new ExamGeneratorService(examRepo, new FakeQuestionPoolRepository(pool), new FakeExamTemplateRepository());

        var request = new BatchGenerateExamRequest(
            "Đề kiểm tra", null, 1, 1, 45, false, false, false, 0,
            3, "ALPHA", "gv1", [EasySection(2)]);

        await service.BatchGenerateAsync(request);

        Assert.Equal(6, examRepo.SavedQuestions.Count);
        Assert.Equal(2, examRepo.SavedQuestions.Select(q => q.QuestionId).Distinct().Count());
    }

    [Fact]
    public async Task GenerateAsync_AlwaysDedupesAcrossSections_RegardlessOfPreventDuplicateFlag()
    {
        var pool = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList(); // đúng bằng 2 section x 2 câu, không có dư
        var examRepo = new FakeExamGeneratorRepository();
        var service = new ExamGeneratorService(examRepo, new FakeQuestionPoolRepository(pool), new FakeExamTemplateRepository());

        // PreventDuplicate=false cố tình — chứng minh dedup giữa section không còn phụ thuộc cờ này.
        var request = new GenerateExamRequest(
            "Đề kiểm tra", null, 1, 1, 45, false, false, false, 0,
            "gv1", [EasySection(2), EasySection(2)]);

        await service.GenerateAsync(request);

        Assert.Equal(4, examRepo.SavedQuestions.Count);
        Assert.Equal(4, examRepo.SavedQuestions.Select(q => q.QuestionId).Distinct().Count());
    }
}
