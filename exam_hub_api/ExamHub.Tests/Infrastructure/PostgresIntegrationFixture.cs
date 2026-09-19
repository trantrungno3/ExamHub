using ExamHub.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace ExamHub.Tests.Infrastructure;

/// <summary>
/// Một PostgreSQL 17 thật (Testcontainers) cho mỗi test class dùng nó, đã apply migration. Test nào
/// gắn fixture này là chạy trên ràng buộc DB thật — partial unique index, transaction, default value —
/// chứ không phải fake in-memory như phần còn lại của project.
///
/// Seed vẫn để riêng trong từng test class: dữ liệu mỗi luồng cần khác nhau, gom vào đây là bắt đầu
/// dựng test framework nội bộ.
/// </summary>
public sealed class PostgresIntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    /// <summary>Connection string của container — dùng khi cần Npgsql thuần thay vì EF.</summary>
    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    /// <summary>DbContext mới trỏ vào container. Mỗi scope logic nên tự tạo context riêng.</summary>
    public AppDbContext CreateContext()
        => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options);

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
