using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Exam</summary>
public class ExamService : IExamService
{
    private readonly IExamRepository _examRepo;
    private readonly IExamQuestionRepository _questionRepo;

    public ExamService(IExamRepository examRepo, IExamQuestionRepository questionRepo)
    {
        _examRepo     = examRepo;
        _questionRepo = questionRepo;
    }

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _examRepo.GetByIdAsync(id, ct);

    public Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default)
        => _examRepo.GetWithQuestionsAsync(id, ct);

    public Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        int? gradeLevelId = null, int? subjectId = null,
        ExamStatusEnum? status = null, string? keyword = null,
        CancellationToken ct = default)
        => _examRepo.GetPagedAsync(page, pageSize, gradeLevelId, subjectId, status, keyword, ct);

    public Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default)
        => _examRepo.GetVariantsAsync(parentExamId, ct);

    public async Task<Exam> CreateAsync(Exam entity, IEnumerable<ExamQuestion> questions, CancellationToken ct = default)
    {
        entity.Id        = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _examRepo.AddAsync(entity, ct);

        var questionList = questions.Select((q, i) =>
        {
            q.Id        = Guid.NewGuid();
            q.ExamId    = entity.Id;
            q.SortOrder = i;
            return q;
        }).ToList();

        if (questionList.Count > 0)
            await _questionRepo.AddRangeAsync(questionList, ct);

        return entity;
    }

    public Task<bool> PublishAsync(Guid id, CancellationToken ct = default)
        => _examRepo.UpdateStatusAsync(id, ExamStatusEnum.Published, ct);

    public Task<bool> ArchiveAsync(Guid id, CancellationToken ct = default)
        => _examRepo.UpdateStatusAsync(id, ExamStatusEnum.Archived, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _examRepo.DeleteByIdAsync(id, ct);

    public async Task<ExamAnalyticsResponse?> GetAnalyticsAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await _examRepo.GetByIdAsync(examId, ct);
        if (exam is null) return null;

        var examQuestions = await _questionRepo.GetByExamWithClassificationAsync(examId, ct);
        var total = examQuestions.Count;

        return new ExamAnalyticsResponse(
            examId,
            total,
            BuildDistribution(examQuestions, total, eq => eq.Question?.CognitiveLevel?.Name ?? "Chưa phân loại"),
            BuildDistribution(examQuestions, total, eq => eq.Question?.DifficultyLevel?.Name ?? "Không xác định"),
            BuildDistribution(examQuestions, total, eq => eq.Question?.Topic?.Name ?? "Không xác định"));
    }

    /// <summary>Nhóm câu hỏi theo nhãn và tính số lượng + phần trăm.</summary>
    private static IReadOnlyList<DistributionItem> BuildDistribution(
        IReadOnlyList<ExamQuestion> examQuestions, int total, Func<ExamQuestion, string> labelSelector)
        => examQuestions
            .GroupBy(labelSelector)
            .Select(g => new DistributionItem(
                g.Key,
                g.Count(),
                total == 0 ? 0 : Math.Round(g.Count() * 100.0 / total, 2)))
            .OrderByDescending(d => d.Count)
            .ToList();
}
