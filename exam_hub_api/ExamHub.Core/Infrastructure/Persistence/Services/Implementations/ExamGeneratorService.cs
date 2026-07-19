using System.Text.Json;
using ExamHub.Core.Application.Services;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>
/// Sinh đề thi tự động — service layer, không phụ thuộc vào infrastructure trực tiếp.
/// </summary>
public class ExamGeneratorService(
    IExamGeneratorRepository repo,
    IQuestionRepository questionRepo,
    IExamTemplateRepository templateRepo) : IExamGeneratorService
{
    /// <inheritdoc/>
    public async Task<Guid> GenerateAsync(GenerateExamRequest request, CancellationToken ct = default)
    {
        var sections   = await ResolveSectionsAsync(request, ct);
        var usedIds    = new HashSet<Guid>();
        var selections = await PickQuestionsAsync(sections, request.SubjectId, request.PreventDuplicate, usedIds, ct);

        if (request.ShuffleQuestions)
            FisherYatesShuffle(selections);

        var examId     = Guid.NewGuid();
        var now        = DateTime.UtcNow;
        var totalScore = request.TotalScore > 0
            ? request.TotalScore
            : sections.Sum(s => s.QuestionCount * s.ScorePerQuestion);

        var exam = new Exam
        {
            Id              = examId,
            ExamTemplateId  = request.ExamTemplateId,
            GradeLevelId    = request.GradeLevelId,
            SubjectId       = request.SubjectId,
            CreatedBy       = request.CreatedBy,
            Title           = request.Title,
            DurationMinutes = request.DurationMinutes,
            TotalScore      = totalScore,
            Status          = ExamStatusEnum.Draft,
            Created       = now,
            Modified       = now
        };

        var questions = selections.Select((s, i) =>
        {
            var sec = sections[s.SectionIndex];
            return new ExamQuestion
            {
                Id              = Guid.NewGuid(),
                ExamId          = examId,
                QuestionId      = s.Question.QuestionId,
                SectionName     = sec.SectionName,
                SortOrder       = i,
                Score           = sec.ScorePerQuestion,
                ContentSnapshot = s.Question.Content,
                AnswersSnapshot = request.ShuffleAnswers
                    ? ShuffleAnswersJson(s.Question.AnswersJson)
                    : s.Question.AnswersJson
            };
        }).ToList();

        return await repo.SaveExamAsync(exam, questions, usedIds, ct);
    }

    /// <inheritdoc/>
    public async Task<BatchGenerateResult> BatchGenerateAsync(
        BatchGenerateExamRequest request, CancellationToken ct = default)
    {
        var baseRequest = new GenerateExamRequest(
            request.Title, request.ExamTemplateId, request.GradeLevelId,
            request.SubjectId, request.DurationMinutes, request.ShuffleQuestions,
            request.ShuffleAnswers, request.PreventDuplicate, request.TotalScore,
            request.CreatedBy, request.Sections);
        var sections = await ResolveSectionsAsync(baseRequest, ct);

        // Pick questions ONCE — all variants share the same question set
        var usedIds  = new HashSet<Guid>();
        var basePool = await PickQuestionsAsync(sections, request.SubjectId, request.PreventDuplicate, usedIds, ct);

        var batchId    = Guid.NewGuid();
        var now        = DateTime.UtcNow;
        var totalScore = request.TotalScore > 0
            ? request.TotalScore
            : sections.Sum(s => s.QuestionCount * s.ScorePerQuestion);
        var baseCode   = batchId.ToString("N")[..6].ToUpper();

        Guid firstExamId = Guid.Empty;
        var exams        = new List<Exam>(request.VariantCount);
        var allQuestions = new List<ExamQuestion>(basePool.Count * request.VariantCount);

        for (int i = 0; i < request.VariantCount; i++)
        {
            var variantCode = GetVariantCode(request.VariantNaming, i);
            var examId      = Guid.NewGuid();
            if (i == 0) firstExamId = examId;

            exams.Add(new Exam
            {
                Id              = examId,
                ExamTemplateId  = request.ExamTemplateId,
                GradeLevelId    = request.GradeLevelId,
                SubjectId       = request.SubjectId,
                CreatedBy       = request.CreatedBy,
                Title           = request.Title,
                ExamCode        = $"{baseCode}-{variantCode}",
                DurationMinutes = request.DurationMinutes,
                TotalScore      = totalScore,
                BatchId         = batchId,
                VariantIndex    = (short)i,
                ParentExamId    = i == 0 ? null : firstExamId,
                Status          = ExamStatusEnum.Draft,
                Created       = now,
                Modified       = now
            });

            var variantSelections = basePool.ToList();
            if (request.ShuffleQuestions) FisherYatesShuffle(variantSelections);

            allQuestions.AddRange(variantSelections.Select((s, idx) =>
            {
                var sec = sections[s.SectionIndex];
                return new ExamQuestion
                {
                    Id              = Guid.NewGuid(),
                    ExamId          = examId,
                    QuestionId      = s.Question.QuestionId,
                    SectionName     = sec.SectionName,
                    SortOrder       = idx,
                    Score           = sec.ScorePerQuestion,
                    ContentSnapshot = s.Question.Content,
                    AnswersSnapshot = request.ShuffleAnswers
                        ? ShuffleAnswersJson(s.Question.AnswersJson)
                        : s.Question.AnswersJson
                };
            }));
        }

        await repo.SaveBatchExamsAsync(exams, allQuestions, usedIds, ct);

        var variants = exams.Select((e, i) =>
            new VariantInfo(e.Id, e.ExamCode, i, GetVariantCode(request.VariantNaming, i))).ToList();

        return new BatchGenerateResult(batchId, variants);
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private async Task<List<(int SectionIndex, PickedQuestion Question)>> PickQuestionsAsync(
        IReadOnlyList<SectionConfig> sections,
        int subjectId,
        bool preventDuplicate,
        HashSet<Guid> usedIds,
        CancellationToken ct)
    {
        // usedIds luôn tích luỹ để tăng usage_count. Chỉ dùng nó làm bộ loại trừ khi chống trùng;
        // nếu cho phép trùng thì mỗi phần chọn độc lập (một câu có thể xuất hiện ở nhiều phần).
        var selections = new List<(int SectionIndex, PickedQuestion Question)>();
        for (int si = 0; si < sections.Count; si++)
        {
            var sec    = sections[si];
            var counts = SplitByDifficulty(sec);
            foreach (var (diffId, n) in counts)
            {
                if (n <= 0) continue;
                var excludeIds = preventDuplicate ? usedIds : (IReadOnlySet<Guid>)EmptyGuidSet;
                var picked = await questionRepo.PickRandomAsync(
                    sec.TopicId, subjectId, sec.QuestionTypeId, (int)diffId, n, excludeIds, sec.CognitiveLevelId, ct);
                if (picked.Count < n)
                    throw new InsufficientQuestionsException(
                        sec.TopicId, (int)diffId, sec.CognitiveLevelId, n, picked.Count);
                foreach (var q in picked)
                {
                    usedIds.Add(q.QuestionId);
                    selections.Add((si, q));
                }
            }
        }
        return selections;
    }

    private static readonly HashSet<Guid> EmptyGuidSet = [];

    /// <summary>
    /// Nếu request có ExamTemplateId và một số section chưa có CognitiveLevelId,
    /// load template để lấy CognitiveLevelId theo index section.
    /// </summary>
    private async Task<IReadOnlyList<SectionConfig>> ResolveSectionsAsync(
        GenerateExamRequest request, CancellationToken ct)
    {
        if (!request.ExamTemplateId.HasValue || request.Sections.All(s => s.CognitiveLevelId.HasValue))
            return request.Sections;

        var template = await templateRepo.GetWithSectionsAsync(request.ExamTemplateId.Value, ct);
        if (template is null || template.Sections.Count == 0)
            return request.Sections;

        var resolved = new List<SectionConfig>(request.Sections.Count);
        for (int i = 0; i < request.Sections.Count; i++)
        {
            var sec = request.Sections[i];
            if (sec.CognitiveLevelId.HasValue)
            {
                resolved.Add(sec);
                continue;
            }
            var templateSec = i < template.Sections.Count ? template.Sections[i] : null;
            resolved.Add(templateSec?.CognitiveLevelId is { } cogId
                ? sec with { CognitiveLevelId = cogId }
                : sec);
        }
        return resolved;
    }

    /// <summary>
    /// Floor cho 3 mức đầu; VeryHard nhận phần dư để tổng luôn = QuestionCount.
    /// </summary>
    private static List<(DifficultyLevelEnum Diff, int Count)> SplitByDifficulty(SectionConfig s)
    {
        int n     = s.QuestionCount;
        int easy  = (int)Math.Floor(n * s.PctEasy   / 100m);
        int med   = (int)Math.Floor(n * s.PctMedium / 100m);
        int hard  = (int)Math.Floor(n * s.PctHard   / 100m);
        int vhard = n - easy - med - hard;
        return
        [
            (DifficultyLevelEnum.Easy,     easy),
            (DifficultyLevelEnum.Medium,   med),
            (DifficultyLevelEnum.Hard,     hard),
            (DifficultyLevelEnum.VeryHard, vhard),
        ];
    }

    private static void FisherYatesShuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static string GetVariantCode(string naming, int index) =>
        naming == "ALPHA"
            ? ((char)('A' + index)).ToString()
            : (index + 1).ToString("D2");

    private static string? ShuffleAnswersJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return json;
        var answers = JsonSerializer.Deserialize<List<JsonElement>>(json);
        if (answers is null || answers.Count <= 1) return json;
        FisherYatesShuffle(answers);
        return JsonSerializer.Serialize(answers);
    }
}
