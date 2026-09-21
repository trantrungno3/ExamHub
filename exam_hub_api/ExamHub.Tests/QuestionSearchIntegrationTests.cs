using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;
using ExamHub.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Tìm kiếm câu hỏi trên PostgreSQL thật: semantics phải giữ nguyên như `ILIKE '%keyword%'` (khớp
/// một phần từ, không phân biệt hoa thường) và plan query phải dùng index trigram thay vì quét bảng.
/// </summary>
public class QuestionSearchIntegrationTests(PostgresIntegrationFixture fixture)
    : IClassFixture<PostgresIntegrationFixture>
{
    private const int NoiseRows = 50_000;
    private static readonly SemaphoreSlim SeedLock = new(1, 1);
    private static bool _seeded;

    private sealed record Ids(int TopicId, int QuestionTypeId, int DifficultyLevelId);

    /// <summary>
    /// Seed một lần cho cả class: cần vài nghìn dòng mới đủ để PostgreSQL chọn index thay vì
    /// sequential scan — với bảng bé thì seq scan mới là plan đúng, và bắt nó dùng index bằng
    /// enable_seqscan=off thì không chứng minh được gì.
    /// </summary>
    private async Task<Ids> SeedAsync()
    {
        await SeedLock.WaitAsync();
        try
        {
            await using var db = fixture.CreateContext();
            var existing = await db.Set<Topic>().FirstOrDefaultAsync(t => t.Name == "Chủ đề search");
            if (_seeded && existing is not null)
                return new Ids(existing.Id, await FirstIdAsync<QuestionType>(db), await FirstIdAsync<DifficultyLevel>(db));

            var gradeLevel = new GradeLevel { Name = "Khối search", GradeNumber = 90 };
            var subject = new Subject { Name = "Toán search", Code = $"MS-{Guid.NewGuid():N}"[..18], GradeLevel = gradeLevel };
            var topic = new Topic { Name = "Chủ đề search", Subject = subject };
            var questionType = new QuestionType { Code = $"QT-{Guid.NewGuid():N}"[..10], Name = "Trắc nghiệm" };
            var difficulty = new DifficultyLevel { Code = $"DL-{Guid.NewGuid():N}"[..10], Name = "Dễ", ScoreWeight = 1m };
            db.AddRange(gradeLevel, subject, topic, questionType, difficulty);
            await db.SaveChangesAsync();

            db.AddRange(
                NewQuestion(topic, questionType, difficulty, "Giải phương trình bậc hai ALGEBRA cơ bản"),
                NewQuestion(topic, questionType, difficulty, "Bài tập algebra nâng cao"),
                NewQuestion(topic, questionType, difficulty, "Câu hỏi không liên quan đến từ khoá"));
            await db.SaveChangesAsync();

            // Nhiễu chèn bằng generate_series: 50k dòng qua EF AddRange mất hàng chục giây, và cần
            // đủ lớn để planner thấy seq scan đắt hơn bitmap index scan.
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO questions (id, topic_id, question_type_id, difficulty_level_id, content, content_plain, tags)
                SELECT gen_random_uuid(), {0}, {1}, {2},
                       '<p>Câu hỏi nhiễu số ' || g || ' về hình học phẳng</p>',
                       'Câu hỏi nhiễu số ' || g || ' về hình học phẳng',
                       ARRAY[]::text[]
                FROM generate_series(1, {3}) g;
                """,
                topic.Id, questionType.Id, difficulty.Id, NoiseRows);
            _seeded = true;
            return new Ids(topic.Id, questionType.Id, difficulty.Id);
        }
        finally
        {
            SeedLock.Release();
        }
    }

    private static async Task<int> FirstIdAsync<T>(DbContext db) where T : class
        => (int)typeof(T).GetProperty("Id")!.GetValue(await db.Set<T>().FirstAsync())!;

    private static Question NewQuestion(Topic topic, QuestionType type, DifficultyLevel difficulty, string content)
        => new()
        {
            Topic = topic,
            QuestionType = type,
            DifficultyLevel = difficulty,
            Content = $"<p>{content}</p>",
            ContentPlain = content,
        };

    [Fact]
    public async Task Keyword_search_is_case_insensitive_and_matches_partial_words()
    {
        await SeedAsync();
        await using var db = fixture.CreateContext();
        var repo = new QuestionRepository(db, null!, null!);

        var (upper, upperTotal) = await repo.GetPagedAsync(1, 20, keyword: "ALGEBRA");
        var (lower, lowerTotal) = await repo.GetPagedAsync(1, 20, keyword: "algebra");
        // Một phần từ phải khớp — đây là lý do giữ ILIKE thay vì plainto_tsquery.
        var (partial, partialTotal) = await repo.GetPagedAsync(1, 20, keyword: "lgebr");

        Assert.Equal(2, upperTotal);
        Assert.Equal(upperTotal, lowerTotal);
        Assert.Equal(upperTotal, partialTotal);
        Assert.All(upper, q => Assert.Contains("algebra", q.ContentPlain!, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(2, lower.Count);
        Assert.Equal(2, partial.Count);
    }

    [Fact]
    public async Task Empty_keyword_returns_a_normal_page()
    {
        await SeedAsync();
        await using var db = fixture.CreateContext();
        var repo = new QuestionRepository(db, null!, null!);

        var (items, total) = await repo.GetPagedAsync(1, 20, keyword: null);
        var (blankItems, blankTotal) = await repo.GetPagedAsync(1, 20, keyword: "   ");

        Assert.Equal(20, items.Count);
        Assert.True(total >= NoiseRows);
        Assert.Equal(total, blankTotal);
        Assert.Equal(items.Count, blankItems.Count);
    }

    [Fact]
    public async Task Special_characters_do_not_break_the_query()
    {
        await SeedAsync();
        await using var db = fixture.CreateContext();
        var repo = new QuestionRepository(db, null!, null!);

        foreach (var keyword in new[] { "100%", "_", "'; DROP TABLE questions; --", "\\", "a%_b" })
        {
            var (items, total) = await repo.GetPagedAsync(1, 5, keyword: keyword);
            Assert.True(total >= 0);
            Assert.True(items.Count <= 5);
        }

        // Bảng vẫn còn sau khi thử chuỗi injection.
        await using var verify = fixture.CreateContext();
        Assert.True(await verify.Set<Question>().AnyAsync());
    }

    [Fact]
    public async Task Search_plan_uses_the_trigram_index_instead_of_a_sequential_scan()
    {
        await SeedAsync();
        await using var conn = new NpgsqlConnection(fixture.ConnectionString);
        await conn.OpenAsync();
        await using (var analyze = new NpgsqlCommand("ANALYZE questions;", conn))
            await analyze.ExecuteNonQueryAsync();

        await using var cmd = new NpgsqlCommand(
            "EXPLAIN (ANALYZE, BUFFERS) SELECT id FROM questions WHERE content_plain ILIKE '%algebra%';",
            conn);
        var plan = new List<string>();
        await using (var reader = await cmd.ExecuteReaderAsync())
            while (await reader.ReadAsync())
                plan.Add(reader.GetString(0));
        var text = string.Join('\n', plan);

        Assert.Contains("ix_questions_content_plain_trgm", text);
        Assert.DoesNotContain("Seq Scan on questions", text);
    }
}
