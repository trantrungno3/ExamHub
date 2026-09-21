using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using ExamHub.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Phân trang danh sách bài nộp trên PostgreSQL thật. Trước đây list-by-session và list-by-student
/// trả về toàn bộ bảng: một kỳ thi 2000 học sinh là một response 2000 bản ghi cho mỗi lần mở màn
/// chấm bài.
/// </summary>
public class ExamSubmissionPaginationTests(PostgresIntegrationFixture fixture)
    : IClassFixture<PostgresIntegrationFixture>
{
    private const int SubmissionCount = 25;
    private static int _gradeNumber = 300;

    /// <summary>Một kỳ thi với 25 bài nộp của 25 học sinh, Created cách nhau 1 phút.</summary>
    private async Task<(Guid SessionId, Guid StudentId)> SeedAsync()
    {
        await using var db = fixture.CreateContext();

        var gradeLevel = new GradeLevel { Name = "Khối paging", GradeNumber = (short)Interlocked.Increment(ref _gradeNumber) };
        var subject = new Subject { Name = "Toán paging", Code = $"PG-{Guid.NewGuid():N}"[..18], GradeLevel = gradeLevel };
        var exam = new Exam { Title = "Đề paging", Subject = subject, GradeLevel = gradeLevel, Status = ExamStatusEnum.Published };
        var session = new ExamSession
        {
            Title = "Kỳ thi paging", Subject = subject, GradeLevel = gradeLevel,
            OpenAt = DateTime.UtcNow.AddHours(-1), CloseAt = DateTime.UtcNow.AddHours(1),
            MaxAttempts = 1, PickMode = ExamSessionPickModeEnum.Random, Status = ExamSessionStatusEnum.Published,
        };
        db.AddRange(gradeLevel, subject, exam, session);
        await db.SaveChangesAsync();

        var baseTime = DateTime.UtcNow.AddHours(-5);
        var firstStudent = Guid.Empty;
        for (var i = 0; i < SubmissionCount; i++)
        {
            var studentId = Guid.NewGuid();
            if (i == 0) firstStudent = studentId;
            db.ExamSubmissions.Add(new ExamSubmission
            {
                SessionId = session.Id, ExamId = exam.Id, StudentId = studentId,
                AttemptNo = 1, Status = SubmissionStatusEnum.Submitted,
                Created = baseTime.AddMinutes(i),
            });
        }
        await db.SaveChangesAsync();

        return (session.Id, firstStudent);
    }

    private ExamSubmissionService ServiceFor(ExamHub.Core.Infrastructure.Persistence.AppDbContext db)
        => new(
            new ExamSubmissionRepository(db),
            new SubmissionAnswerRepository(db),
            new ExamQuestionRepository(db),
            null!,
            new ExamSessionRepository(db));

    [Fact]
    public async Task Session_page_one_and_two_do_not_overlap_and_report_the_full_total()
    {
        var (sessionId, _) = await SeedAsync();
        await using var db = fixture.CreateContext();
        var service = ServiceFor(db);

        var first = await service.GetPageBySessionAsync(sessionId, 1, 10);
        var second = await service.GetPageBySessionAsync(sessionId, 2, 10);
        var third = await service.GetPageBySessionAsync(sessionId, 3, 10);

        Assert.Equal(SubmissionCount, first.Total);
        Assert.Equal(10, first.Items.Count);
        Assert.Equal(10, second.Items.Count);
        Assert.Equal(5, third.Items.Count);
        Assert.Empty(first.Items.Select(x => x.Id).Intersect(second.Items.Select(x => x.Id)));
        // Mới nhất trước.
        Assert.True(first.Items[0].Created >= first.Items[^1].Created);
        Assert.True(first.Items[^1].Created >= second.Items[0].Created);
    }

    [Fact]
    public async Task Page_beyond_the_end_is_empty_but_still_reports_the_total()
    {
        var (sessionId, _) = await SeedAsync();
        await using var db = fixture.CreateContext();

        var page = await ServiceFor(db).GetPageBySessionAsync(sessionId, 99, 10);

        Assert.Empty(page.Items);
        Assert.Equal(SubmissionCount, page.Total);
    }

    [Fact]
    public async Task Service_clamps_page_and_page_size_regardless_of_caller()
    {
        var (sessionId, _) = await SeedAsync();
        await using var db = fixture.CreateContext();
        var service = ServiceFor(db);

        // page 0 sẽ cho OFFSET âm nếu không clamp; pageSize khổng lồ kéo cả bảng.
        var clamped = await service.GetPageBySessionAsync(sessionId, 0, 100_000);

        Assert.Equal(1, clamped.Page);
        Assert.Equal(100, clamped.PageSize);
        Assert.Equal(SubmissionCount, clamped.Items.Count);
    }

    [Fact]
    public async Task Student_page_is_scoped_to_that_student()
    {
        var (_, studentId) = await SeedAsync();
        await using var db = fixture.CreateContext();

        var page = await ServiceFor(db).GetPageByStudentAsync(studentId, 1, 10);

        Assert.Single(page.Items);
        Assert.Equal(1, page.Total);
        Assert.Equal(studentId, page.Items[0].StudentId);
    }

    [Fact]
    public async Task Paged_reads_are_not_tracked()
    {
        var (sessionId, _) = await SeedAsync();
        await using var db = fixture.CreateContext();

        await ServiceFor(db).GetPageBySessionAsync(sessionId, 1, 10);

        Assert.Empty(db.ChangeTracker.Entries());
    }
}
