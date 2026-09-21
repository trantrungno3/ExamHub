using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Migration là nguồn schema duy nhất cho dev bootstrap, nên phải chứng minh hai điều trên
/// PostgreSQL thật: (1) database trống migrate được tới schema hiện tại, (2) database đang ở
/// migration trước upgrade lên mới nhất mà không mất dữ liệu đã có.
/// Mỗi test tự dựng container riêng vì cả hai đều cần database ở trạng thái khởi đầu khác nhau.
/// </summary>
public class MigrationBootstrapIntegrationTests
{
    private static AppDbContext ContextFor(string connectionString)
        => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options);

    private static async Task<bool> RelationExistsAsync(string connectionString, string name)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT EXISTS (SELECT 1 FROM pg_class WHERE relname = @name)", conn);
        cmd.Parameters.AddWithValue("name", name);
        return (bool)(await cmd.ExecuteScalarAsync())!;
    }

    [Fact]
    public async Task Empty_database_migrates_to_the_current_schema()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        var cs = container.GetConnectionString();

        await using (var db = ContextFor(cs))
        {
            await db.Database.MigrateAsync();
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        }

        // Bảng lõi và hai partial unique index của bài nộp phải tồn tại sau migrate.
        Assert.True(await RelationExistsAsync(cs, "exam_submissions"));
        Assert.True(await RelationExistsAsync(cs, "exam_sessions"));
        Assert.True(await RelationExistsAsync(cs, "ix_exam_submissions_session_id_student_id"));
        Assert.True(await RelationExistsAsync(cs, "ix_exam_submissions_session_id_student_id_attempt_no"));
    }

    [Fact]
    public async Task Upgrading_from_the_previous_migration_keeps_existing_rows()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        var cs = container.GetConnectionString();

        var migrations = ContextFor(cs).Database.GetMigrations().ToList();
        Assert.True(migrations.Count >= 2, "Cần ít nhất hai migration để kiểm thử upgrade.");
        var previous = migrations[^2];

        var gradeLevelId = 0;
        await using (var db = ContextFor(cs))
        {
            var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
            await migrator.MigrateAsync(previous);

            var grade = new GradeLevel { Name = "Lớp 10", GradeNumber = 10 };
            db.Add(grade);
            await db.SaveChangesAsync();
            gradeLevelId = grade.Id;
        }

        await using (var db = ContextFor(cs))
        {
            await db.Database.MigrateAsync();
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
            // Dữ liệu ghi ở migration cũ vẫn còn sau khi upgrade.
            Assert.NotNull(await db.Set<GradeLevel>().FirstOrDefaultAsync(g => g.Id == gradeLevelId));
        }
    }
}
