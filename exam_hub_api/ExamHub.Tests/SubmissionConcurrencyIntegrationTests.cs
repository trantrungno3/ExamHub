using ExamHub.Core.DataTransferObjects.ExamSession;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Infrastructure.Persistence;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using TVT.Core;
using TVT.Core.Enums;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Starts a real postgres:17-alpine container (Testcontainers) once per test class and applies
/// EF migrations against it, so tests using this fixture exercise the real DB constraints
/// (partial unique indexes) instead of the in-memory fakes used everywhere else in this project.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    private string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        return new AppDbContext(options);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

/// <summary>
/// Concurrent starts must leave one in_progress submission for the session and student.
/// </summary>
public class SubmissionConcurrencyIntegrationTests : IClassFixture<PostgresContainerFixture>
{
    private static int _gradeNumber;
    private readonly PostgresContainerFixture _fixture;

    public SubmissionConcurrencyIntegrationTests(PostgresContainerFixture fixture)
        => _fixture = fixture;

    private async Task<(Guid sessionId, Guid studentId, Guid examId)> SeedAsync()
    {
        await using var db = _fixture.CreateContext();

        var gradeLevel = new GradeLevel { Name = "Test grade", GradeNumber = (short)Interlocked.Increment(ref _gradeNumber) };
        var subject = new Subject { Name = "Toán", Code = $"MATH-{Guid.NewGuid():N}"[..20], GradeLevel = gradeLevel };
        var school = new School { Name = "Trường test", Code = $"SCH-{Guid.NewGuid():N}"[..20] };
        var cohort = new Cohort { Name = "Khoá test", School = school, StartYear = 2024, EndYear = 2027, GradeStart = 10 };
        var studentId = Guid.NewGuid();
        var member = new CohortMember { Cohort = cohort, StudentId = studentId, IsActive = true };
        var exam = new Exam { Title = "Đề test", Subject = subject, GradeLevel = gradeLevel, Status = ExamStatusEnum.Published };
        var session = new ExamSession
        {
            Title = "Kỳ thi test", Subject = subject, GradeLevel = gradeLevel,
            OpenAt = DateTime.UtcNow.AddHours(-1), CloseAt = DateTime.UtcNow.AddHours(1),
            MaxAttempts = 1, PickMode = ExamSessionPickModeEnum.Random, Status = ExamSessionStatusEnum.Published,
        };
        var poolExam = new ExamSessionExam { Exam = exam };
        var assignment = new ExamSessionAssignment { Cohort = cohort };
        session.Exams.Add(poolExam);
        session.Assignments.Add(assignment);

        db.AddRange(gradeLevel, subject, school, cohort, member, exam, session);
        await db.SaveChangesAsync();

        return (session.Id, studentId, exam.Id);
    }

    [Fact]
    public async Task Concurrent_StartAsync_calls_leave_exactly_one_in_progress_submission()
    {
        var (sessionId, studentId, examId) = await SeedAsync();

        using var barrier = new Barrier(2);

        async Task<RequestResponse<StartSessionResponse>?> AttemptStart()
        {
            await using var db = _fixture.CreateContext();
            var sessionRepo = new ExamSessionRepository(db);
            var examRepo = new ExamRepository(db);
            var service = new ExamSessionService(sessionRepo, examRepo);

            Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(30)), "Both start calls must reach the barrier.");
            try
            {
                return await service.StartAsync(sessionId, studentId, null, "student1");
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return null;
            }
        }

        var results = await Task.WhenAll(Task.Run(AttemptStart), Task.Run(AttemptStart));
        Assert.Contains(results, x => x?.Status == RequestResponseStatus.Success);
        Assert.All(results.Where(x => x is not null), x => Assert.Equal(RequestResponseStatus.Success, x!.Status));

        await using var verifyDb = _fixture.CreateContext();
        var rows = await verifyDb.ExamSubmissions
            .Where(x => x.SessionId == sessionId && x.StudentId == studentId)
            .ToListAsync();

        Assert.Single(rows);
        Assert.Single(rows, x => x.Status == SubmissionStatusEnum.InProgress);
        Assert.Equal(examId, rows[0].ExamId);
    }

    [Theory]
    [InlineData(SubmissionStatusEnum.Submitted, SubmissionStatusEnum.Graded, 1)]
    [InlineData(SubmissionStatusEnum.InProgress, SubmissionStatusEnum.InProgress, 2)]
    public async Task Migration_rejects_duplicate_attempts_and_concurrent_active_attempts(
        SubmissionStatusEnum firstStatus, SubmissionStatusEnum secondStatus, short secondAttempt)
    {
        var (sessionId, studentId, examId) = await SeedAsync();
        await using var db = _fixture.CreateContext();
        db.ExamSubmissions.Add(new ExamSubmission
        {
            SessionId = sessionId, StudentId = studentId, ExamId = examId, AttemptNo = 1, Status = firstStatus
        });
        await db.SaveChangesAsync();
        db.ExamSubmissions.Add(new ExamSubmission
        {
            SessionId = sessionId, StudentId = studentId, ExamId = examId, AttemptNo = secondAttempt, Status = secondStatus
        });

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
    }

    [Fact]
    public async Task Migration_allows_completed_attempts_and_submissions_without_a_session()
    {
        var (sessionId, studentId, examId) = await SeedAsync();
        await using var db = _fixture.CreateContext();
        db.ExamSubmissions.AddRange(
            new ExamSubmission { SessionId = sessionId, StudentId = studentId, ExamId = examId, AttemptNo = 1, Status = SubmissionStatusEnum.Submitted },
            new ExamSubmission { SessionId = sessionId, StudentId = studentId, ExamId = examId, AttemptNo = 2, Status = SubmissionStatusEnum.Graded },
            new ExamSubmission { SessionId = sessionId, StudentId = studentId, ExamId = examId, AttemptNo = 3 },
            new ExamSubmission { StudentId = studentId, ExamId = examId },
            new ExamSubmission { StudentId = studentId, ExamId = examId });

        await db.SaveChangesAsync();
        Assert.Equal(5, await db.ExamSubmissions.CountAsync(x => x.StudentId == studentId));
    }
}
