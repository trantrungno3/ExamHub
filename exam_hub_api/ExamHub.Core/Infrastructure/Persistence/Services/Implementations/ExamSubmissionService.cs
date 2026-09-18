using ExamHub.Core.Application.Grading;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Exam;
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
    private readonly IUserManagementService _userService;
    private readonly IExamSessionRepository _sessionRepo;

    public ExamSubmissionService(
        IExamSubmissionRepository submissionRepo,
        ISubmissionAnswerRepository answerRepo,
        IExamQuestionRepository examQuestionRepo,
        IUserManagementService userService,
        IExamSessionRepository sessionRepo)
    {
        _submissionRepo   = submissionRepo;
        _answerRepo       = answerRepo;
        _examQuestionRepo = examQuestionRepo;
        _userService      = userService;
        _sessionRepo      = sessionRepo;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<Guid, StudentDirectoryEntry>> GetStudentDirectoryAsync(
        IReadOnlyCollection<Guid> studentIds, CancellationToken ct = default)
    {
        var ids = studentIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, StudentDirectoryEntry>();

        var classNames = await _submissionRepo.GetStudentClassNamesAsync(ids, ct);

        var idSet = ids.ToHashSet();
        var names = _userService.GetList()
            .Where(u => idSet.Contains(u.Id))
            .GroupBy(u => u.Id)
            .ToDictionary(g => g.Key, g => g.First().DisplayName);

        return ids.ToDictionary(id => id, id => new StudentDirectoryEntry(
            id,
            names.TryGetValue(id, out var n) ? n : null,
            classNames.TryGetValue(id, out var c) ? c : null));
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

    public Task<IReadOnlyList<ExamSubmission>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
        => _submissionRepo.GetBySessionAsync(sessionId, ct);

    public Task<IReadOnlyList<ExamSubmission>> GetBySessionAndStudentAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => _submissionRepo.GetBySessionAndStudentAsync(sessionId, studentId, ct);

    public async Task<ExamSubmission> SubmitAsync(
        ExamSubmission submission,
        IEnumerable<SubmissionAnswer> answers,
        Guid currentUserId,
        CancellationToken ct = default)
    {
        if (submission.Id != Guid.Empty)
        {
            var existing = await _submissionRepo.GetByIdAsync(submission.Id, ct)
                ?? throw new InvalidOperationException("Không tìm thấy bài làm.");
            if (existing.StudentId != currentUserId)
                throw new UnauthorizedAccessException("Bạn không có quyền nộp bài làm này.");
            if (existing.Status != SubmissionStatusEnum.InProgress)
                throw new InvalidOperationException("Bài làm đã được nộp.");
            await EnsureSessionOpenAsync(existing, ct);
            return await SubmitInProgressAsync(existing, answers, ct);
        }

        // Luồng đề trực tiếp (giữ nguyên): tạo bản nộp mới.
        submission.StudentId = currentUserId;
        await EnsureSessionOpenAsync(submission, ct);
        submission.Id          = Guid.NewGuid();
        submission.SubmittedAt = DateTime.UtcNow;
        submission.Status      = SubmissionStatusEnum.Submitted;
        submission.Created   = DateTime.UtcNow;
        submission.Modified   =  DateTime.UtcNow;

        var answerList = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = submission.Id;
            a.EssayContent = a.EssayContent?.Trim();
            return a;
        }).ToList();

        var examQuestions = await _examQuestionRepo.GetByExamAsync(submission.ExamId, ct);
        ApplyAutoGrade(examQuestions, answerList);
        submission.TotalScore = answerList.Sum(a => a.ScoreEarned);
        submission.Status     = SubmissionGrading.DecideStatus(examQuestions);

        // Insert submission + insert đáp án là một đơn vị: nếu ghi đáp án lỗi thì bản nộp rỗng
        // cũng không được tồn tại.
        return await _submissionRepo.ExecuteInTransactionAsync(async token =>
        {
            // Điểm/trạng thái đã tính xong trước khi insert, nên không cần UPDATE ngay sau ADD.
            await _submissionRepo.AddAsync(submission, token);
            if (answerList.Count > 0)
                await _answerRepo.AddRangeAsync(answerList, token);
            return submission;
        }, ct);
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
        var answerList = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = existing.Id;
            return a;
        }).ToList();

        // Đổi trạng thái sang Submitted và thay đáp án phải cùng sống hoặc cùng chết: nếu chỉ
        // ReplaceForSubmissionAsync lỗi, bài sẽ bị đánh dấu đã nộp mà đáp án đã bị xoá trắng.
        // Việc mutate entity cũng nằm trong transaction để rollback trả bài về đúng in_progress.
        return await _submissionRepo.ExecuteInTransactionAsync(async token =>
        {
            var now = DateTime.UtcNow;
            existing.SubmittedAt     = now;
            existing.Status          = SubmissionStatusEnum.Submitted;
            existing.DurationSeconds = (int)Math.Max(0, (now - existing.StartedAt).TotalSeconds);
            existing.Modified        = now;

            // Chấm theo đề đã khoá của bản nộp, không theo ExamId gửi lên.
            var examQuestions = await _examQuestionRepo.GetByExamAsync(existing.ExamId, token);
            ApplyAutoGrade(examQuestions, answerList);
            existing.TotalScore = answerList.Sum(a => a.ScoreEarned);
            existing.Status     = SubmissionGrading.DecideStatus(examQuestions);

            await _submissionRepo.UpdateAsync(existing, token);

            // Bản in_progress CÓ THỂ đã có sẵn đáp án do autosave (SaveProgressAsync) ghi trước
            // đó. Vì vậy phải xoá sạch rồi ghi lại (ReplaceForSubmissionAsync = delete-then-
            // insert, cùng ngữ nghĩa autosave đang dùng) — nếu chỉ AddRange sẽ sinh bản ghi trùng
            // cho mỗi câu: vi phạm UNIQUE (submission_id, exam_question_id), và nếu lọt qua thì
            // màn chấm hiện mỗi câu hai lần còn FinalizeAsync cộng điểm sai. Gọi cả khi danh sách
            // rỗng để không sót lại đáp án autosave cũ.
            await _answerRepo.ReplaceForSubmissionAsync(existing.Id, answerList, token);
            return existing;
        }, ct);
    }

    /// <summary>Chấm tự động câu trắc nghiệm dựa trên danh sách examQuestions đã nạp.</summary>
    private static void ApplyAutoGrade(
        IReadOnlyList<ExamQuestion> examQuestions, IReadOnlyList<SubmissionAnswer> answers)
    {
        var byId = examQuestions.ToDictionary(eq => eq.Id);
        foreach (var answer in answers)
        {
            if (answer.SelectedAnswerIds is not { Length: > 0 } selected) continue;
            if (!byId.TryGetValue(answer.ExamQuestionId, out var examQuestion)) continue;

            var correctIds = SubmissionGrading.CorrectAnswerIds(examQuestion.AnswersSnapshot);
            var isCorrect  = correctIds.Count > 0 && correctIds.SetEquals(selected);
            answer.IsCorrect   = isCorrect;
            answer.ScoreEarned = isCorrect ? examQuestion.Score ?? 1m : 0m;
        }
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

    public async Task SaveProgressAsync(
        Guid submissionId, Guid currentUserId, IEnumerable<SubmissionAnswer> answers, CancellationToken ct = default)
    {
        var existing = await _submissionRepo.GetByIdAsync(submissionId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy bài làm.");
        // Chỉ chủ nhân bài làm được ghi đè. ReplaceForSubmissionAsync là delete-then-insert nên
        // nếu thiếu kiểm tra này, bất kỳ tài khoản nào biết/đoán được submissionId đều có thể
        // xoá trắng bài làm đang thi của học sinh khác.
        if (existing.StudentId != currentUserId)
            throw new UnauthorizedAccessException("Bạn không có quyền lưu bài làm này.");
        if (existing.Status != SubmissionStatusEnum.InProgress)
            throw new InvalidOperationException("Bài làm đã nộp, không thể lưu tạm.");
        await EnsureSessionOpenAsync(existing, ct);

        var list = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = submissionId;
            a.EssayContent = a.EssayContent?.Trim();
            return a;
        }).ToList();

        // ReplaceForSubmissionAsync xoá trước rồi mới insert: không có transaction thì insert lỗi
        // sẽ để học sinh mất trắng đáp án đã lưu.
        await _submissionRepo.ExecuteInTransactionAsync<object?>(async token =>
        {
            await _answerRepo.ReplaceForSubmissionAsync(submissionId, list, token);
            return null;
        }, ct);
    }

    private async Task EnsureSessionOpenAsync(ExamSubmission submission, CancellationToken ct)
    {
        if (submission.SessionId is null) return;

        var session = await _sessionRepo.GetByIdAsync(submission.SessionId.Value, ct)
            ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
        var now = DateTime.UtcNow;
        if (session.Status == ExamSessionStatusEnum.Closed || now > session.CloseAt)
            throw new InvalidOperationException("Kỳ thi đã đóng.");
        if (session.Status != ExamSessionStatusEnum.Published)
            throw new InvalidOperationException("Kỳ thi chưa mở.");
        if (now < session.OpenAt)
            throw new InvalidOperationException("Kỳ thi chưa đến giờ mở.");
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
