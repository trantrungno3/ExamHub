using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho ExamSubmission</summary>
public class ExamSubmissionService : IExamSubmissionService
{
    private readonly IExamSubmissionRepository _submissionRepo;
    private readonly ISubmissionAnswerRepository _answerRepo;

    public ExamSubmissionService(
        IExamSubmissionRepository submissionRepo,
        ISubmissionAnswerRepository answerRepo)
    {
        _submissionRepo = submissionRepo;
        _answerRepo     = answerRepo;
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
        submission.Id          = Guid.NewGuid();
        submission.SubmittedAt = DateTime.UtcNow;
        submission.Status      = SubmissionStatusEnum.Submitted;
        submission.CreatedAt   = DateTime.UtcNow;

        await _submissionRepo.AddAsync(submission, ct);

        var answerList = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = submission.Id;
            return a;
        }).ToList();

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);

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
