using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using ExamHub.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TVT.Core.Enums;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Luồng làm bài đi hết vòng trên PostgreSQL thật: start → autosave → resume → submit → submit lại.
/// Dùng service và repository thật, không fake — mục tiêu là wiring giữa service, transaction runner
/// và ràng buộc DB, chứ không lặp lại unit test.
///
/// Không đi qua HTTP: dựng Program bằng WebApplicationFactory đòi Mongo, Redis, MinIO và RabbitMQ
/// cùng chạy (AddServicesApi wire cả năm), biến test này thành test hạ tầng. Phần plumbing của
/// controller đã có test riêng bằng DefaultHttpContext.
/// </summary>
public class SubmissionJourneyIntegrationTests(PostgresIntegrationFixture fixture)
    : IClassFixture<PostgresIntegrationFixture>
{
    private static int _gradeNumber = 100;

    private sealed record Seed(Guid SessionId, Guid StudentId, Guid ExamId, Guid QuestionA, Guid QuestionB);

    /// <summary>Một kỳ thi published, một đề 2 câu trắc nghiệm, một học sinh được giao.</summary>
    private async Task<Seed> SeedAsync()
    {
        await using var db = fixture.CreateContext();

        var gradeLevel = new GradeLevel { Name = "Khối test", GradeNumber = (short)Interlocked.Increment(ref _gradeNumber) };
        var subject = new Subject { Name = "Toán", Code = $"MATH-{Guid.NewGuid():N}"[..20], GradeLevel = gradeLevel };
        var school = new School { Name = "Trường test", Code = $"SCH-{Guid.NewGuid():N}"[..20] };
        var cohort = new Cohort { Name = "Khoá test", School = school, StartYear = 2024, EndYear = 2027, GradeStart = 10 };
        var studentId = Guid.NewGuid();
        var member = new CohortMember { Cohort = cohort, StudentId = studentId, IsActive = true };

        var correctA = Guid.NewGuid();
        var correctB = Guid.NewGuid();
        var exam = new Exam
        {
            Title = "Đề test", Subject = subject, GradeLevel = gradeLevel, Status = ExamStatusEnum.Published,
        };
        // exam_questions.question_id là FK bắt buộc về questions, nên phải seed cả câu hỏi gốc
        // cùng topic/loại/độ khó của nó.
        var topic = new Topic { Name = "Chủ đề test", Subject = subject };
        var questionType = new QuestionType { Code = $"MCQ-{Guid.NewGuid():N}"[..10], Name = "Trắc nghiệm" };
        var difficulty = new DifficultyLevel { Code = $"EASY-{Guid.NewGuid():N}"[..10], Name = "Dễ", ScoreWeight = 1m };
        var sourceA = new Question { Topic = topic, QuestionType = questionType, DifficultyLevel = difficulty, Content = "1 + 1 = ?" };
        var sourceB = new Question { Topic = topic, QuestionType = questionType, DifficultyLevel = difficulty, Content = "2 + 2 = ?" };

        var questionA = new ExamQuestion
        {
            Exam = exam, Question = sourceA, ContentSnapshot = "1 + 1 = ?", Score = 1m,
            AnswersSnapshot = $"[{{\"id\":\"{correctA}\",\"is_correct\":true}},{{\"id\":\"{Guid.NewGuid()}\",\"is_correct\":false}}]",
        };
        var questionB = new ExamQuestion
        {
            Exam = exam, Question = sourceB, ContentSnapshot = "2 + 2 = ?", Score = 1m,
            AnswersSnapshot = $"[{{\"id\":\"{correctB}\",\"is_correct\":true}},{{\"id\":\"{Guid.NewGuid()}\",\"is_correct\":false}}]",
        };

        var session = new ExamSession
        {
            Title = "Kỳ thi test", Subject = subject, GradeLevel = gradeLevel,
            OpenAt = DateTime.UtcNow.AddHours(-1), CloseAt = DateTime.UtcNow.AddHours(1),
            MaxAttempts = 1, PickMode = ExamSessionPickModeEnum.Random, Status = ExamSessionStatusEnum.Published,
        };
        session.Exams.Add(new ExamSessionExam { Exam = exam });
        session.Assignments.Add(new ExamSessionAssignment { Cohort = cohort });

        db.AddRange(gradeLevel, subject, school, cohort, member, topic, questionType, difficulty,
            sourceA, sourceB, exam, questionA, questionB, session);
        await db.SaveChangesAsync();

        return new Seed(session.Id, studentId, exam.Id, questionA.Id, questionB.Id);
    }


    [Fact]
    public async Task Student_starts_autosaves_resumes_then_submits_once()
    {
        var seed = await SeedAsync();
        var correctA = await CorrectAnswerIdAsync(seed.QuestionA);

        // 1. Start — tạo đúng một lượt in_progress.
        Guid submissionId;
        await using (var db = fixture.CreateContext())
        {
            var service = new ExamSessionService(new ExamSessionRepository(db), new ExamRepository(db));
            var started = await service.StartAsync(seed.SessionId, seed.StudentId, null, "student1");
            Assert.Equal(RequestResponseStatus.Success, started.Status);
            submissionId = started.Data!.SubmissionId;
        }

        // 2. Autosave hai vòng — vòng sau thay hoàn toàn vòng trước, không nhân đôi dòng.
        await SaveProgressAsync(submissionId, seed.StudentId, seed.QuestionA, Guid.NewGuid());
        await SaveProgressAsync(submissionId, seed.StudentId, seed.QuestionA, correctA);

        // 3. Resume — đọc lại đúng đáp án đã lưu gần nhất.
        await using (var db = fixture.CreateContext())
        {
            var saved = await db.Set<SubmissionAnswer>()
                .Where(a => a.SubmissionId == submissionId)
                .ToListAsync();
            var answer = Assert.Single(saved);
            Assert.Equal(seed.QuestionA, answer.ExamQuestionId);
            Assert.Equal([correctA], answer.SelectedAnswerIds!);

            var resumed = await db.ExamSubmissions.SingleAsync(x => x.Id == submissionId);
            Assert.Equal(SubmissionStatusEnum.InProgress, resumed.Status);
        }

        // 4. Submit — chấm tự động, chuyển trạng thái, đáp án được thay chứ không nhân đôi.
        await using (var db = fixture.CreateContext())
        {
            var service = NewSubmissionService(db);
            var existing = await db.ExamSubmissions.SingleAsync(x => x.Id == submissionId);
            var result = await service.SubmitAsync(
                new ExamSubmission { Id = submissionId, ExamId = existing.ExamId, StudentId = seed.StudentId },
                [new SubmissionAnswer { ExamQuestionId = seed.QuestionA, SelectedAnswerIds = [correctA] }],
                seed.StudentId);

            Assert.NotEqual(SubmissionStatusEnum.InProgress, result.Status);
            Assert.Equal(1m, result.TotalScore);
        }

        // 5. Submit lần hai bị từ chối, và kết quả lần đầu còn nguyên.
        await using (var db = fixture.CreateContext())
        {
            var service = NewSubmissionService(db);
            var existing = await db.ExamSubmissions.SingleAsync(x => x.Id == submissionId);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitAsync(
                new ExamSubmission { Id = submissionId, ExamId = existing.ExamId, StudentId = seed.StudentId },
                [],
                seed.StudentId));
        }

        await using (var db = fixture.CreateContext())
        {
            var rows = await db.ExamSubmissions
                .Where(x => x.SessionId == seed.SessionId && x.StudentId == seed.StudentId)
                .ToListAsync();
            var submission = Assert.Single(rows);
            Assert.NotEqual(SubmissionStatusEnum.InProgress, submission.Status);
            Assert.Equal(1m, submission.TotalScore);
            Assert.Single(await db.Set<SubmissionAnswer>().Where(a => a.SubmissionId == submissionId).ToListAsync());
        }
    }

    private async Task<Guid> CorrectAnswerIdAsync(Guid examQuestionId)
    {
        await using var db = fixture.CreateContext();
        var snapshot = (await db.Set<ExamQuestion>().SingleAsync(q => q.Id == examQuestionId)).AnswersSnapshot!;
        return ExamHub.Core.Application.Grading.SubmissionGrading.CorrectAnswerIds(snapshot).Single();
    }

    private async Task SaveProgressAsync(Guid submissionId, Guid studentId, Guid examQuestionId, Guid selected)
    {
        await using var db = fixture.CreateContext();
        await NewSubmissionService(db).SaveProgressAsync(
            submissionId,
            studentId,
            [new SubmissionAnswer { ExamQuestionId = examQuestionId, SelectedAnswerIds = [selected] }]);
    }

    private static ExamSubmissionService NewSubmissionService(ExamHub.Core.Infrastructure.Persistence.AppDbContext db)
        => new(
            new ExamSubmissionRepository(db),
            new SubmissionAnswerRepository(db),
            new ExamQuestionRepository(db),
            null!,
            new ExamSessionRepository(db));
}
