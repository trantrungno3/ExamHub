using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ExamHub.Core.Application.Submissions;
using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Converters;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Xunit;

namespace ExamHub.Tests;

// ── Hand-rolled fakes (no mocking library in this repo — xunit only) ──────

/// <summary>Fake IExamSubmissionRepository backed by an in-memory list.</summary>
file sealed class FakeSubmissionRepository : IExamSubmissionRepository
{
    public List<ExamSubmission> Submissions { get; } = [];

    public Task<ExamSubmission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Submissions.FirstOrDefault(s => s.Id == id));

    public Task<ExamSubmission> AddAsync(ExamSubmission entity, CancellationToken ct = default)
    {
        Submissions.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(ExamSubmission entity, CancellationToken ct = default)
    {
        var i = Submissions.FindIndex(s => s.Id == entity.Id);
        if (i >= 0) Submissions[i] = entity; else Submissions.Add(entity);
        return Task.CompletedTask;
    }

    /// <summary>Gọi khi transaction mở; trả về action hoàn tác để chạy nếu operation ném.</summary>
    public Func<Action>? OnBeginTransaction { get; set; }

    /// <summary>Chụp danh sách và các field service mutate in-place; trả action phục hồi.</summary>
    public Action Snapshot()
    {
        var rows = Submissions
            .Select(s => (Entity: s, s.Status, s.SubmittedAt, s.TotalScore, s.DurationSeconds))
            .ToList();
        return () =>
        {
            foreach (var row in rows)
            {
                row.Entity.Status          = row.Status;
                row.Entity.SubmittedAt     = row.SubmittedAt;
                row.Entity.TotalScore      = row.TotalScore;
                row.Entity.DurationSeconds = row.DurationSeconds;
            }
            Submissions.Clear();
            Submissions.AddRange(rows.Select(r => r.Entity));
        };
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        var rollback = OnBeginTransaction?.Invoke();
        try
        {
            return await operation(ct);
        }
        catch
        {
            rollback?.Invoke();
            throw;
        }
    }

    public Task<ExamSubmission?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<ExamSubmission?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamSubmission>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamSubmission>> GetBySessionAndStudentAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyDictionary<Guid, string>> GetStudentClassNamesAsync(IReadOnlyCollection<Guid> studentIds, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamSubmission>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamSubmission>> GetAsync(Expression<Func<ExamSubmission, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<ExamSubmission?> FirstOrDefaultAsync(Expression<Func<ExamSubmission, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<ExamSubmission, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<ExamSubmission, bool>>? predicate = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<ExamSubmission> entities, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteAsync(ExamSubmission entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

public class ExamSubmissionResponseTests
{
    [Fact]
    public void InProgress_uses_elapsed_seconds_at_response_time()
    {
        var now = new DateTime(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc);
        var submission = new ExamSubmission
        {
            Id = Guid.NewGuid(), ExamId = Guid.NewGuid(), StudentId = Guid.NewGuid(),
            Status = SubmissionStatusEnum.InProgress, StartedAt = now.AddSeconds(-125)
        };

        var response = ExamSubmissionResponse.FromEntity(submission, now: now);

        Assert.Equal(125, response.DurationSeconds);
    }

    [Fact]
    public void Finished_submission_keeps_persisted_duration_seconds()
    {
        var now = new DateTime(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc);
        var submission = new ExamSubmission
        {
            Id = Guid.NewGuid(), ExamId = Guid.NewGuid(), StudentId = Guid.NewGuid(),
            Status = SubmissionStatusEnum.Submitted,
            StartedAt = now.AddMinutes(-20), DurationSeconds = 321
        };

        var response = ExamSubmissionResponse.FromEntity(submission, now: now);

        Assert.Equal(321, response.DurationSeconds);
    }
}

/// <summary>
/// Fake ISubmissionAnswerRepository. Mô phỏng ràng buộc UNIQUE (submission_id, exam_question_id)
/// của bảng submission_answers: ghi trùng ném lỗi giống Postgres, để test bắt được lỗi C3
/// (autosave + nộp bài sinh bản ghi trùng) thay vì âm thầm chấp nhận.
/// </summary>
file sealed class FakeAnswerRepository(List<string> callLog) : ISubmissionAnswerRepository
{
    public List<SubmissionAnswer> Answers { get; } = [];

    private void AddRespectingUnique(IEnumerable<SubmissionAnswer> items)
    {
        foreach (var a in items)
        {
            if (Answers.Any(x => x.SubmissionId == a.SubmissionId && x.ExamQuestionId == a.ExamQuestionId))
                throw new InvalidOperationException(
                    $"UNIQUE (submission_id, exam_question_id) violated: {a.SubmissionId}/{a.ExamQuestionId}");
            Answers.Add(a);
        }
    }

    public Task<IReadOnlyList<SubmissionAnswer>> GetBySubmissionAsync(Guid submissionId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SubmissionAnswer>>(Answers.Where(a => a.SubmissionId == submissionId).ToList());

    public Task DeleteBySubmissionAsync(Guid submissionId, CancellationToken ct = default)
    {
        callLog.Add($"answers-delete:{submissionId}");
        Answers.RemoveAll(a => a.SubmissionId == submissionId);
        return Task.CompletedTask;
    }

    /// <summary>Nếu set, ghi đáp án sẽ ném — với replace là ném SAU khi đã xoá bản cũ, đúng như
    /// delete-then-insert thật lỗi ở bước insert.</summary>
    public Exception? FailOnWrite { get; set; }

    /// <summary>Chụp danh sách đáp án; trả action phục hồi.</summary>
    public Action Snapshot()
    {
        var rows = Answers.ToList();
        return () =>
        {
            Answers.Clear();
            Answers.AddRange(rows);
        };
    }

    public Task ReplaceForSubmissionAsync(Guid submissionId, IReadOnlyList<SubmissionAnswer> answers, CancellationToken ct = default)
    {
        callLog.Add($"answers-replace:{submissionId}:{answers.Count}");
        Answers.RemoveAll(a => a.SubmissionId == submissionId);
        if (FailOnWrite is { } ex) throw ex;
        foreach (var a in answers) a.SubmissionId = submissionId;
        AddRespectingUnique(answers);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<SubmissionAnswer> entities, CancellationToken ct = default)
    {
        var list = entities.ToList();
        callLog.Add($"answers-addrange:{list.Count}");
        AddRespectingUnique(list);
        return Task.CompletedTask;
    }

    public Task<SubmissionAnswer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Answers.FirstOrDefault(a => a.Id == id));
    public Task<IReadOnlyList<SubmissionAnswer>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<SubmissionAnswer>> GetAsync(Expression<Func<SubmissionAnswer, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<SubmissionAnswer?> FirstOrDefaultAsync(Expression<Func<SubmissionAnswer, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<SubmissionAnswer, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<SubmissionAnswer, bool>>? predicate = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<SubmissionAnswer> AddAsync(SubmissionAnswer entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task UpdateAsync(SubmissionAnswer entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteAsync(SubmissionAnswer entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

/// <summary>Fake IExamQuestionRepository — chỉ GetByExamAsync được dùng khi nộp bài.</summary>
file sealed class FakeExamQuestionRepository : IExamQuestionRepository
{
    public List<ExamQuestion> Questions { get; } = [];

    public Task<IReadOnlyList<ExamQuestion>> GetByExamAsync(Guid examId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ExamQuestion>>(Questions.Where(q => q.ExamId == examId).ToList());

    public Task<IReadOnlyList<ExamQuestion>> GetByExamWithClassificationAsync(Guid examId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteByExamAsync(Guid examId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<bool> ExistsByQuestionAsync(Guid questionId, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<ExamQuestion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamQuestion>> GetAllAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<IReadOnlyList<ExamQuestion>> GetAsync(Expression<Func<ExamQuestion, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<ExamQuestion?> FirstOrDefaultAsync(Expression<Func<ExamQuestion, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<bool> ExistsAsync(Expression<Func<ExamQuestion, bool>> predicate, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> CountAsync(Expression<Func<ExamQuestion, bool>>? predicate = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<ExamQuestion> AddAsync(ExamQuestion entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task AddRangeAsync(IEnumerable<ExamQuestion> entities, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task UpdateAsync(ExamQuestion entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteAsync(ExamQuestion entity, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();
}

// ── C1: trạng thái mới phải hợp lệ với schema Postgres thật ─────────────────

/// <summary>
/// Bản CHECK constraint của cột exam_submissions.status nằm trong database_schema.sql (được
/// compose.yaml mount làm docker-entrypoint-initdb.d/init.sql) chứ KHÔNG có trong migration EF,
/// nên không thể phát hiện lệch bằng build. Test này đọc thẳng file schema để chặn đúng lỗi C1:
/// thêm giá trị enum mà quên cập nhật CHECK ⇒ mọi lần nộp bài tự luận trả về 500.
/// </summary>
public class SubmissionStatusSchemaTests
{
    private static string SchemaPath([CallerFilePath] string thisFile = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "database_schema.sql"));

    /// <summary>Chuỗi được EF ghi xuống DB cho một giá trị enum.</summary>
    private static string Persisted(SubmissionStatusEnum status)
        => (string)new SnakeCaseEnumConverter<SubmissionStatusEnum>().ConvertToProvider(status)!;

    private static string SubmissionsTableDdl()
    {
        var sql = File.ReadAllText(SchemaPath());
        var start = sql.IndexOf("CREATE TABLE public.exam_submissions", StringComparison.Ordinal);
        Assert.True(start >= 0, "Không tìm thấy bảng exam_submissions trong database_schema.sql");
        var end = sql.IndexOf(");", start, StringComparison.Ordinal);
        return sql[start..end];
    }

    [Fact]
    public void Every_submission_status_is_allowed_by_the_check_constraint()
    {
        var ddl = SubmissionsTableDdl();
        var match = Regex.Match(ddl, @"CHECK\s*\(status\s+IN\s*\(([^)]*)\)\)", RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Không tìm thấy CHECK (status IN (...)) cho exam_submissions");

        var allowed = match.Groups[1].Value
            .Split(',')
            .Select(s => s.Trim().Trim('\''))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var status in Enum.GetValues<SubmissionStatusEnum>())
            Assert.Contains(Persisted(status), allowed);
    }

    [Fact]
    public void Every_submission_status_fits_the_declared_column_length()
    {
        var ddl = SubmissionsTableDdl();
        var match = Regex.Match(ddl, @"status\s+VARCHAR\((\d+)\)", RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Không đọc được VARCHAR(n) của cột status");
        var maxLength = int.Parse(match.Groups[1].Value);

        // 20 cũng là giá trị HasMaxLength(20) trong AppDbContext — hai nơi phải khớp nhau.
        Assert.Equal(20, maxLength);
        foreach (var status in Enum.GetValues<SubmissionStatusEnum>())
            Assert.True(Persisted(status).Length <= maxLength,
                $"'{Persisted(status)}' dài {Persisted(status).Length} ký tự > VARCHAR({maxLength})");
    }

    [Fact]
    public void PendingManualGrade_round_trips_through_the_converter()
    {
        var converter = new SnakeCaseEnumConverter<SubmissionStatusEnum>();
        var stored = (string)converter.ConvertToProvider(SubmissionStatusEnum.PendingManualGrade)!;

        Assert.Equal("pending_manual_grade", stored);
        Assert.Equal(SubmissionStatusEnum.PendingManualGrade, converter.ConvertFromProvider(stored));
    }
}

// ── C2: PendingManualGrade phải tính là một lượt đã dùng ────────────────────

/// <summary>
/// Bộ lọc đếm lượt đã dùng chạy trong truy vấn EF (cần DB thật để chạy end-to-end), nên nó được
/// tách thành biểu thức thuần <see cref="SubmissionAttempts.UsedAttemptFilter"/> để test compile
/// và áp lên dữ liệu trong bộ nhớ — đúng biểu thức mà repository truyền cho CountAsync.
/// </summary>
public class UsedAttemptFilterTests
{
    private static readonly Guid SessionId = Guid.NewGuid();
    private static readonly Guid StudentId = Guid.NewGuid();

    private static bool Matches(ExamSubmission s)
        => SubmissionAttempts.UsedAttemptFilter(SessionId, StudentId).Compile()(s);

    private static ExamSubmission Sub(SubmissionStatusEnum status, Guid? sessionId = null, Guid? studentId = null) => new()
    {
        Id = Guid.NewGuid(),
        ExamId = Guid.NewGuid(),
        SessionId = sessionId ?? SessionId,
        StudentId = studentId ?? StudentId,
        Status = status
    };

    [Theory]
    [InlineData(SubmissionStatusEnum.Submitted)]
    [InlineData(SubmissionStatusEnum.Graded)]
    [InlineData(SubmissionStatusEnum.PendingManualGrade)]
    public void Finished_statuses_count_as_a_used_attempt(SubmissionStatusEnum status)
        => Assert.True(Matches(Sub(status)));

    [Fact]
    public void InProgress_does_not_count_as_a_used_attempt()
        => Assert.False(Matches(Sub(SubmissionStatusEnum.InProgress)));

    [Fact]
    public void Filter_is_scoped_to_the_session_and_the_student()
    {
        Assert.False(Matches(Sub(SubmissionStatusEnum.Submitted, sessionId: Guid.NewGuid())));
        Assert.False(Matches(Sub(SubmissionStatusEnum.Submitted, studentId: Guid.NewGuid())));
    }

    [Fact]
    public void Filter_covers_every_declared_status()
    {
        // Chặn đúng lỗi C2: thêm trạng thái kết thúc mới mà quên cập nhật bộ lọc.
        foreach (var status in Enum.GetValues<SubmissionStatusEnum>())
            Assert.Equal(status != SubmissionStatusEnum.InProgress, Matches(Sub(status)));
    }
}

// ── C3 + I5: nộp bài sau autosave, và quyền ghi khi autosave ────────────────

public class ExamSubmissionServiceTests
{
    private static readonly Guid ExamId = Guid.NewGuid();

    private static ExamQuestion Mcq(Guid correctAnswerId, decimal score = 1m) => new()
    {
        Id = Guid.NewGuid(),
        ExamId = ExamId,
        QuestionId = Guid.NewGuid(),
        ContentSnapshot = "Câu trắc nghiệm",
        Score = score,
        AnswersSnapshot = $"[{{\"id\":\"{correctAnswerId}\",\"is_correct\":true}},{{\"id\":\"{Guid.NewGuid()}\",\"is_correct\":false}}]"
    };

    private static ExamQuestion Essay() => new()
    {
        Id = Guid.NewGuid(),
        ExamId = ExamId,
        QuestionId = Guid.NewGuid(),
        ContentSnapshot = "Câu tự luận",
        Score = 2m,
        AnswersSnapshot = "[]" // không có đáp án đúng ⇒ chấm tay
    };

    private static SubmissionAnswer Answer(Guid examQuestionId, Guid[]? selected = null, string? essay = null) => new()
    {
        Id = Guid.NewGuid(),
        ExamQuestionId = examQuestionId,
        SelectedAnswerIds = selected,
        EssayContent = essay
    };

    // Các fake là file-local (theo pattern sẵn có của repo) nên không được xuất hiện trong chữ ký
    // của bất kỳ thành viên nào — mỗi test tự dựng bộ đồ nghề ngay trong thân hàm.
    // IUserManagementService chỉ phục vụ GetStudentDirectoryAsync, không nằm trên luồng nộp
    // bài/lưu tạm, nên truyền null! thay vì fake 12 thành viên không bao giờ được gọi.

    private static ExamSubmission InProgress(Guid studentId) => new()
    {
        Id = Guid.NewGuid(),
        ExamId = ExamId,
        StudentId = studentId,
        SessionId = Guid.NewGuid(),
        Status = SubmissionStatusEnum.InProgress,
        StartedAt = DateTime.UtcNow.AddMinutes(-10)
    };

    private static FakeExamSessionRepository SessionRepoFor(
        ExamSubmission submission,
        bool closedByStatus = false,
        bool expired = false,
        bool upcoming = false)
    {
        var repo = new FakeExamSessionRepository();
        repo.Sessions.Add(new ExamSession
        {
            Id = submission.SessionId!.Value,
            Title = "Kỳ thi",
            SubjectId = 1,
            GradeLevelId = 1,
            Status = closedByStatus ? ExamSessionStatusEnum.Closed : ExamSessionStatusEnum.Published,
            OpenAt = upcoming ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow.AddHours(-1),
            CloseAt = expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddHours(1)
        });
        return repo;
    }

    [Fact]
    public async Task Submit_rolls_back_and_keeps_autosaved_answers_when_answer_write_fails()
    {
        var correct = Guid.NewGuid();
        var q1 = Mcq(correct);
        var submissions = new FakeSubmissionRepository();
        var answerRepo = new FakeAnswerRepository([]);
        var questions = new FakeExamQuestionRepository();
        questions.Questions.Add(q1);
        var student = Guid.NewGuid();
        var existing = InProgress(student);
        submissions.Submissions.Add(existing);
        var service = new ExamSubmissionService(
            submissions, answerRepo, questions, null!, SessionRepoFor(existing));

        await service.SaveProgressAsync(existing.Id, student, [Answer(q1.Id, [correct])]);
        Assert.Single(answerRepo.Answers);

        // Fake transaction runner: chụp trạng thái khi mở, phục hồi nếu operation ném.
        submissions.OnBeginTransaction = () =>
        {
            var restoreAnswers = answerRepo.Snapshot();
            var restoreSubmissions = submissions.Snapshot();
            return () => { restoreSubmissions(); restoreAnswers(); };
        };
        answerRepo.FailOnWrite = new InvalidOperationException("insert đáp án lỗi");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitAsync(
            new ExamSubmission { Id = existing.Id, ExamId = ExamId, StudentId = student },
            [Answer(q1.Id, [correct])],
            student));

        // Đáp án autosave không được mất, và bài vẫn đang làm dở (chưa bị đánh dấu đã nộp).
        Assert.Single(answerRepo.Answers);
        Assert.Equal(SubmissionStatusEnum.InProgress, submissions.Submissions.Single().Status);
        Assert.Null(submissions.Submissions.Single().SubmittedAt);
    }

    [Fact]
    public async Task SaveProgress_rolls_back_and_keeps_previous_answers_when_write_fails()
    {
        var q1 = Mcq(Guid.NewGuid());
        var submissions = new FakeSubmissionRepository();
        var answerRepo = new FakeAnswerRepository([]);
        var questions = new FakeExamQuestionRepository();
        questions.Questions.Add(q1);
        var student = Guid.NewGuid();
        var existing = InProgress(student);
        submissions.Submissions.Add(existing);
        var service = new ExamSubmissionService(
            submissions, answerRepo, questions, null!, SessionRepoFor(existing));

        await service.SaveProgressAsync(existing.Id, student, [Answer(q1.Id, [Guid.NewGuid()], essay: "nháp")]);
        var savedAnswerId = answerRepo.Answers.Single().Id;

        submissions.OnBeginTransaction = () =>
        {
            var restoreAnswers = answerRepo.Snapshot();
            var restoreSubmissions = submissions.Snapshot();
            return () => { restoreSubmissions(); restoreAnswers(); };
        };
        answerRepo.FailOnWrite = new InvalidOperationException("insert đáp án lỗi");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveProgressAsync(existing.Id, student, [Answer(q1.Id, [Guid.NewGuid()])]));

        Assert.Equal(savedAnswerId, answerRepo.Answers.Single().Id);
    }

    [Fact]
    public async Task Submit_after_autosave_does_not_duplicate_answer_rows()
    {
        var correct = Guid.NewGuid();
        var q1 = Mcq(correct);
        var q2 = Essay();
        var submissions = new FakeSubmissionRepository();
        var answerRepo = new FakeAnswerRepository([]);
        var questions = new FakeExamQuestionRepository();
        questions.Questions.AddRange([q1, q2]);
        var student = Guid.NewGuid();
        var existing = InProgress(student);
        submissions.Submissions.Add(existing);
        var service = new ExamSubmissionService(
            submissions, answerRepo, questions, null!, SessionRepoFor(existing));

        // Autosave hai vòng (mô phỏng chu kỳ ~20s của frontend).
        await service.SaveProgressAsync(existing.Id, student,
            [Answer(q1.Id, [Guid.NewGuid()]), Answer(q2.Id, essay: "nháp")]);
        await service.SaveProgressAsync(existing.Id, student,
            [Answer(q1.Id, [correct]), Answer(q2.Id, essay: "nháp 2")]);
        Assert.Equal(2, answerRepo.Answers.Count);

        // Nộp bài trên chính bản in_progress đó.
        await service.SubmitAsync(
            new ExamSubmission { Id = existing.Id, ExamId = ExamId, StudentId = student },
            [Answer(q1.Id, [correct]), Answer(q2.Id, essay: "bài làm cuối")],
            student);

        // Đúng một dòng cho mỗi câu — không nhân đôi.
        Assert.Equal(2, answerRepo.Answers.Count);
        Assert.Single(answerRepo.Answers, a => a.ExamQuestionId == q1.Id);
        Assert.Single(answerRepo.Answers, a => a.ExamQuestionId == q2.Id);
        Assert.All(answerRepo.Answers, a => Assert.Equal(existing.Id, a.SubmissionId));

        // Chỉ giữ bản nộp cuối, và điểm tổng chỉ cộng một lần (1 điểm câu trắc nghiệm đúng).
        Assert.Equal("bài làm cuối", answerRepo.Answers.Single(a => a.ExamQuestionId == q2.Id).EssayContent);
        Assert.Equal(1m, submissions.Submissions.Single().TotalScore);
        Assert.Equal(SubmissionStatusEnum.PendingManualGrade, submissions.Submissions.Single().Status);
    }

    [Fact]
    public async Task Submit_with_no_answers_clears_previously_autosaved_rows()
    {
        var q1 = Mcq(Guid.NewGuid());
        var submissions = new FakeSubmissionRepository();
        var answerRepo = new FakeAnswerRepository([]);
        var questions = new FakeExamQuestionRepository();
        questions.Questions.Add(q1);
        var student = Guid.NewGuid();
        var existing = InProgress(student);
        submissions.Submissions.Add(existing);
        var service = new ExamSubmissionService(
            submissions, answerRepo, questions, null!, SessionRepoFor(existing));

        await service.SaveProgressAsync(existing.Id, student, [Answer(q1.Id, [Guid.NewGuid()])]);
        Assert.Single(answerRepo.Answers);

        await service.SubmitAsync(
            new ExamSubmission { Id = existing.Id, ExamId = ExamId, StudentId = student }, [], student);

        Assert.Empty(answerRepo.Answers);
        Assert.Equal(0m, submissions.Submissions.Single().TotalScore);
    }

    [Fact]
    public async Task SaveProgress_rejects_a_user_who_does_not_own_the_submission()
    {
        var q1 = Mcq(Guid.NewGuid());
        var submissions = new FakeSubmissionRepository();
        var answerRepo = new FakeAnswerRepository([]);
        var questions = new FakeExamQuestionRepository();
        questions.Questions.Add(q1);
        var owner = Guid.NewGuid();
        var existing = InProgress(owner);
        submissions.Submissions.Add(existing);
        var service = new ExamSubmissionService(
            submissions, answerRepo, questions, null!, SessionRepoFor(existing));
        await service.SaveProgressAsync(existing.Id, owner, [Answer(q1.Id, [Guid.NewGuid()])]);

        var attacker = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.SaveProgressAsync(existing.Id, attacker, []));

        Assert.Equal("Bạn không có quyền lưu bài làm này.", ex.Message);
        // Quan trọng: ReplaceForSubmissionAsync là delete-then-insert ⇒ phải chặn TRƯỚC khi ghi,
        // nếu không một body rỗng sẽ xoá trắng bài làm của học sinh khác.
        Assert.Single(answerRepo.Answers);
    }

    [Fact]
    public async Task SaveProgress_still_rejects_a_submission_that_is_no_longer_in_progress()
    {
        var q1 = Mcq(Guid.NewGuid());
        var submissions = new FakeSubmissionRepository();
        var answerRepo = new FakeAnswerRepository([]);
        var questions = new FakeExamQuestionRepository();
        questions.Questions.Add(q1);
        var student = Guid.NewGuid();
        var existing = InProgress(student);
        existing.Status = SubmissionStatusEnum.PendingManualGrade;
        submissions.Submissions.Add(existing);
        var service = new ExamSubmissionService(
            submissions, answerRepo, questions, null!, SessionRepoFor(existing));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SaveProgressAsync(existing.Id, student, [Answer(q1.Id, [Guid.NewGuid()])]));

        Assert.Equal("Bài làm đã nộp, không thể lưu tạm.", ex.Message);
    }

    [Theory]
    [InlineData(true, false, false, "Kỳ thi đã đóng.")]
    [InlineData(false, true, false, "Kỳ thi đã đóng.")]
    [InlineData(false, false, true, "Kỳ thi chưa đến giờ mở.")]
    public async Task SaveProgress_rejects_unavailable_session(
        bool closedByStatus, bool expired, bool upcoming, string expectedMessage)
    {
        var student = Guid.NewGuid();
        var existing = InProgress(student);
        var submissions = new FakeSubmissionRepository();
        submissions.Submissions.Add(existing);
        var answers = new FakeAnswerRepository([]);
        var service = new ExamSubmissionService(
            submissions, answers, new FakeExamQuestionRepository(), null!,
            SessionRepoFor(existing, closedByStatus, expired, upcoming));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SaveProgressAsync(existing.Id, student, []));

        Assert.Equal(expectedMessage, error.Message);
        Assert.Empty(answers.Answers);
    }

    [Theory]
    [InlineData(true, false, false, "Kỳ thi đã đóng.")]
    [InlineData(false, true, false, "Kỳ thi đã đóng.")]
    [InlineData(false, false, true, "Kỳ thi chưa đến giờ mở.")]
    public async Task SubmitAsync_rejects_unavailable_session_without_mutating_submission(
        bool closedByStatus, bool expired, bool upcoming, string expectedMessage)
    {
        var student = Guid.NewGuid();
        var existing = InProgress(student);
        var submissions = new FakeSubmissionRepository();
        submissions.Submissions.Add(existing);
        var answers = new FakeAnswerRepository([]);
        var service = new ExamSubmissionService(
            submissions, answers, new FakeExamQuestionRepository(), null!,
            SessionRepoFor(existing, closedByStatus, expired, upcoming));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitAsync(new ExamSubmission { Id = existing.Id }, [], student));

        Assert.Equal(expectedMessage, error.Message);
        Assert.Equal(SubmissionStatusEnum.InProgress, existing.Status);
        Assert.Null(existing.SubmittedAt);
        Assert.Empty(answers.Answers);
    }

    [Fact]
    public async Task SubmitAsync_rejects_user_who_does_not_own_in_progress_submission()
    {
        var owner = Guid.NewGuid();
        var existing = InProgress(owner);
        var submissions = new FakeSubmissionRepository();
        submissions.Submissions.Add(existing);
        var service = new ExamSubmissionService(
            submissions, new FakeAnswerRepository([]), new FakeExamQuestionRepository(), null!,
            SessionRepoFor(existing));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.SubmitAsync(new ExamSubmission { Id = existing.Id }, [], Guid.NewGuid()));
    }
}
