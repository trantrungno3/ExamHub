using System.Text.Json;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho ExamSubmission</summary>
public class ExamSubmissionService : IExamSubmissionService
{
    private readonly IExamSubmissionRepository _submissionRepo;
    private readonly ISubmissionAnswerRepository _answerRepo;
    private readonly IExamQuestionRepository _examQuestionRepo;

    public ExamSubmissionService(
        IExamSubmissionRepository submissionRepo,
        ISubmissionAnswerRepository answerRepo,
        IExamQuestionRepository examQuestionRepo)
    {
        _submissionRepo   = submissionRepo;
        _answerRepo       = answerRepo;
        _examQuestionRepo = examQuestionRepo;
    }

    public Task<ExamSubmission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _submissionRepo.GetByIdAsync(id, ct);

    public Task<ExamSubmission?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => _submissionRepo.GetWithAnswersAsync(id, ct);

    public Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId, CancellationToken ct = default)
        => _submissionRepo.GetByExamAsync(examId, ct);

    public Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => _submissionRepo.GetByStudentAsync(studentId, ct);

    public Task<ExamSubmission?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default)
        => _submissionRepo.GetByExamAndStudentAsync(examId, studentId, ct);

    public async Task<ExamSubmission> SubmitAsync(
        ExamSubmission submission,
        IEnumerable<SubmissionAnswer> answers,
        CancellationToken ct = default)
    {
        // Luồng kỳ thi: có sẵn bản in_progress (đã bốc/khoá đề) → cập nhật thay vì tạo mới.
        if (submission.Id != Guid.Empty)
        {
            var existing = await _submissionRepo.GetByIdAsync(submission.Id, ct);
            if (existing is not null && existing.Status == SubmissionStatusEnum.InProgress)
                return await SubmitInProgressAsync(existing, answers, ct);
        }

        // Luồng đề trực tiếp (giữ nguyên): tạo bản nộp mới.
        submission.Id          = Guid.NewGuid();
        submission.SubmittedAt = DateTime.UtcNow;
        submission.Status      = SubmissionStatusEnum.Submitted;
        submission.Created   = DateTime.UtcNow;
        submission.Modified   =  DateTime.UtcNow;

        await _submissionRepo.AddAsync(submission, ct);

        var answerList = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = submission.Id;
            return a;
        }).ToList();

        await AutoGradeObjectiveAsync(submission.ExamId, answerList, ct);

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);

        return submission;
    }

    /// <summary>
    /// Nộp bài cho bản in_progress của kỳ thi: chấm trắc nghiệm tự động, tính điểm/thời gian,
    /// chuyển trạng thái sang Submitted và UPDATE (giữ nguyên Id, session_id, attempt_no).
    /// </summary>
    private async Task<ExamSubmission> SubmitInProgressAsync(
        ExamSubmission existing,
        IEnumerable<SubmissionAnswer> answers,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        existing.SubmittedAt     = now;
        existing.Status          = SubmissionStatusEnum.Submitted;
        existing.DurationSeconds = (int)Math.Max(0, (now - existing.StartedAt).TotalSeconds);
        existing.Modified        = now;

        var answerList = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = existing.Id;
            return a;
        }).ToList();

        // Chấm theo đề đã khoá của bản nộp, không theo ExamId gửi lên.
        await AutoGradeObjectiveAsync(existing.ExamId, answerList, ct);
        existing.TotalScore = answerList.Sum(a => a.ScoreEarned);

        await _submissionRepo.UpdateAsync(existing, ct);

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);

        return existing;
    }

    /// <summary>
    /// Chấm tự động các câu trắc nghiệm (có <see cref="SubmissionAnswer.SelectedAnswerIds"/>):
    /// so khớp tập đáp án đã chọn với tập đáp án đúng trong snapshot.
    /// Câu tự luận (chỉ có EssayContent) giữ nguyên IsCorrect = null để giáo viên chấm tay.
    /// </summary>
    private async Task AutoGradeObjectiveAsync(
        Guid examId, IReadOnlyList<SubmissionAnswer> answers, CancellationToken ct)
    {
        if (!answers.Any(a => a.SelectedAnswerIds is { Length: > 0 }))
            return;

        var examQuestions = await _examQuestionRepo.GetByExamAsync(examId, ct);
        var byId = examQuestions.ToDictionary(eq => eq.Id);

        foreach (var answer in answers)
        {
            if (answer.SelectedAnswerIds is not { Length: > 0 } selected)
                continue;
            if (!byId.TryGetValue(answer.ExamQuestionId, out var examQuestion))
                continue;

            var correctIds = CorrectAnswerIdsFromSnapshot(examQuestion.AnswersSnapshot);
            var isCorrect  = correctIds.Count > 0 && correctIds.SetEquals(selected);

            answer.IsCorrect   = isCorrect;
            answer.ScoreEarned = isCorrect ? examQuestion.Score ?? 1m : 0m;
        }
    }

    /// <summary>Trích tập UUID đáp án đúng từ snapshot JSON [{id, is_correct, ...}].</summary>
    private static HashSet<Guid> CorrectAnswerIdsFromSnapshot(string? snapshotJson)
    {
        var result = new HashSet<Guid>();
        if (string.IsNullOrWhiteSpace(snapshotJson))
            return result;

        using var doc = JsonDocument.Parse(snapshotJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var el in doc.RootElement.EnumerateArray())
        {
            if (el.TryGetProperty("is_correct", out var ic) && ic.ValueKind == JsonValueKind.True &&
                el.TryGetProperty("id", out var idEl) && idEl.TryGetGuid(out var id))
                result.Add(id);
        }
        return result;
    }

    public async Task<ExamSubmission> FinalizeAsync(
        Guid submissionId, Guid gradedBy, CancellationToken ct = default)
    {
        var submission = await _submissionRepo.GetByIdAsync(submissionId, ct)
            ?? throw new KeyNotFoundException($"ExamSubmission '{submissionId}' not found.");

        var answers = await _answerRepo.GetBySubmissionAsync(submissionId, ct);

        submission.TotalScore = answers.Sum(a => a.ScoreEarned);
        submission.Status     = SubmissionStatusEnum.Graded;
        submission.Modified = DateTime.UtcNow;

        await _submissionRepo.UpdateAsync(submission, ct);
        return submission;
    }

    public async Task GradeAnswerAsync(
        Guid submissionAnswerId,
        decimal scoreEarned,
        bool isCorrect,
        string? feedback,
        Guid gradedBy,
        CancellationToken ct = default)
    {
        var answer = await _answerRepo.GetByIdAsync(submissionAnswerId, ct)
            ?? throw new KeyNotFoundException($"SubmissionAnswer '{submissionAnswerId}' not found.");

        answer.ScoreEarned = scoreEarned;
        answer.IsCorrect   = isCorrect;
        answer.Feedback    = feedback;
        answer.GradedBy    = gradedBy;

        await _answerRepo.UpdateAsync(answer, ct);
    }
}
