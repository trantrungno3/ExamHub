using ExamHub.Core.Application.Services;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>
/// Sinh đề thi tự động — service layer, không phụ thuộc vào infrastructure trực tiếp.
/// </summary>
public class ExamGeneratorService(IExamGeneratorRepository repo, IQuestionRepository questionRepo) : IExamGeneratorService
{
    /// <inheritdoc/>
    public async Task<Guid> GenerateAsync(GenerateExamRequest request, CancellationToken ct = default)
    {
        var usedIds    = new HashSet<Guid>();
        var selections = new List<(int SectionIndex, PickedQuestion Question)>();

        // ── 1. Pick câu hỏi ngẫu nhiên (ORDER BY RANDOM() qua Dapper) ────
        for (int si = 0; si < request.Sections.Count; si++)
        {
            var sec    = request.Sections[si];
            var counts = SplitByDifficulty(sec);

            foreach (var (diffId, n) in counts)
            {
                if (n <= 0) continue;

                var picked = await questionRepo.PickRandomAsync(
                    sec.TopicId, sec.QuestionTypeId, (int)diffId, n, usedIds, ct);

                if (picked.Count < n)
                    throw new InvalidOperationException(
                        $"Không đủ câu hỏi trong pool: topic={sec.TopicId}, difficulty={diffId}. " +
                        $"Cần {n}, tìm được {picked.Count}.");

                foreach (var q in picked)
                {
                    usedIds.Add(q.QuestionId);
                    selections.Add((si, q));
                }
            }
        }

        // ── 2. Shuffle toàn bộ nếu yêu cầu ──────────────────────────────
        if (request.ShuffleQuestions)
            FisherYatesShuffle(selections);

        // ── 3. Build domain entities ──────────────────────────────────────
        var examId     = Guid.NewGuid();
        var now        = DateTime.UtcNow;
        var totalScore = request.Sections.Sum(s => s.QuestionCount * s.ScorePerQuestion);

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
            CreatedAt       = now,
            UpdatedAt       = now
        };

        var questions = selections.Select((s, i) =>
        {
            var sec = request.Sections[s.SectionIndex];
            return new ExamQuestion
            {
                Id              = Guid.NewGuid(),
                ExamId          = examId,
                QuestionId      = s.Question.QuestionId,
                SectionName     = sec.SectionName,
                SortOrder       = i,
                Score           = sec.ScorePerQuestion,
                ContentSnapshot = s.Question.Content,
                AnswersSnapshot = s.Question.AnswersJson
            };
        }).ToList();

        // ── 4. Lưu nguyên tử qua repository ──────────────────────────────
        return await repo.SaveExamAsync(exam, questions, usedIds, ct);
    }

    // ── Private helpers ──────────────────────────────────────────────────

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
}
