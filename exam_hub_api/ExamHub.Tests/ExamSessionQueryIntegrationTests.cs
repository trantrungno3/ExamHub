using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;
using ExamHub.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Query danh sách kỳ thi trên PostgreSQL thật. Một session có nhiều exam và nhiều assignment, nên
/// include cả hai collection trong một query là tích Descartes: mỗi session sinh exams × assignments
/// dòng. Test ở đây khoá ba thứ: không duplicate item, `Total` đếm session chứ không đếm row join,
/// và read path không bị EF tracking.
/// </summary>
public class ExamSessionQueryIntegrationTests(PostgresIntegrationFixture fixture)
    : IClassFixture<PostgresIntegrationFixture>
{
    private static int _gradeNumber = 200;

    /// <summary>3 session, session mới nhất có 2 exam và 2 assignment.</summary>
    private async Task<(int SubjectId, Guid RichSessionId)> SeedAsync()
    {
        await using var db = fixture.CreateContext();

        var gradeLevel = new GradeLevel { Name = "Khối query", GradeNumber = (short)Interlocked.Increment(ref _gradeNumber) };
        var subject = new Subject { Name = "Toán query", Code = $"QQ-{Guid.NewGuid():N}"[..18], GradeLevel = gradeLevel };
        var school = new School { Name = "Trường query", Code = $"SQ-{Guid.NewGuid():N}"[..18] };
        var cohortA = new Cohort { Name = "Khoá A", School = school, StartYear = 2024, EndYear = 2027, GradeStart = 10 };
        var cohortB = new Cohort { Name = "Khoá B", School = school, StartYear = 2025, EndYear = 2028, GradeStart = 10 };
        var examOne = new Exam { Title = "Đề 1", Subject = subject, GradeLevel = gradeLevel, Status = ExamStatusEnum.Published };
        var examTwo = new Exam { Title = "Đề 2", Subject = subject, GradeLevel = gradeLevel, Status = ExamStatusEnum.Published };

        ExamSession NewSession(string title, DateTime created) => new()
        {
            Title = title, Subject = subject, GradeLevel = gradeLevel,
            OpenAt = DateTime.UtcNow.AddHours(-1), CloseAt = DateTime.UtcNow.AddHours(1),
            MaxAttempts = 1, PickMode = ExamSessionPickModeEnum.Random,
            Status = ExamSessionStatusEnum.Published, Created = created,
        };

        var oldest = NewSession("Kỳ thi cũ nhất", DateTime.UtcNow.AddDays(-3));
        var middle = NewSession("Kỳ thi giữa", DateTime.UtcNow.AddDays(-2));
        var rich = NewSession("Kỳ thi nhiều đề nhiều lớp", DateTime.UtcNow.AddDays(-1));

        // 2 exam × 2 assignment = 4 dòng join cho cùng một session.
        rich.Exams.Add(new ExamSessionExam { Exam = examOne });
        rich.Exams.Add(new ExamSessionExam { Exam = examTwo });
        rich.Assignments.Add(new ExamSessionAssignment { Cohort = cohortA });
        rich.Assignments.Add(new ExamSessionAssignment { Cohort = cohortB });

        db.AddRange(gradeLevel, subject, school, cohortA, cohortB, examOne, examTwo, oldest, middle, rich);
        await db.SaveChangesAsync();

        return (subject.Id, rich.Id);
    }

    [Fact]
    public async Task Sessions_with_many_relations_are_not_duplicated_and_total_counts_sessions()
    {
        var (subjectId, richSessionId) = await SeedAsync();
        await using var db = fixture.CreateContext();
        var repo = new ExamSessionRepository(db);

        var (items, total) = await repo.GetPagedAsync(1, 10, subjectId, null, null, null);

        Assert.Equal(3, total);
        Assert.Equal(3, items.Count);
        var rich = Assert.Single(items, s => s.Id == richSessionId);
        // Navigation vẫn phải nạp đủ dù đã page trước khi load quan hệ.
        Assert.Equal(2, rich.Exams.Count);
        Assert.Equal(2, rich.Assignments.Count);
        Assert.NotNull(rich.Subject);
        Assert.NotNull(rich.GradeLevel);
    }

    [Fact]
    public async Task Paging_keeps_newest_first_and_does_not_overlap()
    {
        var (subjectId, richSessionId) = await SeedAsync();
        await using var db = fixture.CreateContext();
        var repo = new ExamSessionRepository(db);

        var (firstPage, total) = await repo.GetPagedAsync(1, 2, subjectId, null, null, null);
        var (secondPage, _) = await repo.GetPagedAsync(2, 2, subjectId, null, null, null);

        Assert.Equal(3, total);
        Assert.Equal(2, firstPage.Count);
        Assert.Single(secondPage);
        Assert.Equal(richSessionId, firstPage[0].Id);
        Assert.Empty(firstPage.Select(x => x.Id).Intersect(secondPage.Select(x => x.Id)));
    }

    [Fact]
    public async Task Read_path_does_not_track_entities()
    {
        var (subjectId, _) = await SeedAsync();
        await using var db = fixture.CreateContext();
        var repo = new ExamSessionRepository(db);

        var (items, _) = await repo.GetPagedAsync(1, 10, subjectId, null, null, null);

        // Tracking cả danh sách read-only là giữ snapshot cho từng entity + quan hệ, tốn bộ nhớ và
        // mở đường cho SaveChanges ghi ngoài ý muốn.
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.All(items, s => Assert.Equal(EntityState.Detached, db.Entry(s).State));
    }
}
