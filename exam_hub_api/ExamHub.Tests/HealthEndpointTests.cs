using System.Text;
using ExamHub.API.Health;
using ExamHub.Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Testcontainers.PostgreSql;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Liveness/readiness. Không dùng WebApplicationFactory: boot Program thật sẽ dựng cả DI của Mongo,
/// Redis, MinIO, RabbitMQ và fail-fast CORS, biến test health thành test hạ tầng. Thay vào đó test
/// đúng hai thứ quyết định hành vi endpoint — probe database thật, và response writer (chỗ duy nhất
/// có thể rò cấu hình ra ngoài).
/// </summary>
public class HealthEndpointTests
{
    private static AppDbContext ContextFor(string connectionString)
        => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options);

    private static async Task<string> WriteAsync(HealthReport report)
    {
        var http = new DefaultHttpContext();
        http.Response.Body = new MemoryStream();
        await HealthResponse.WriteAsync(http, report);
        http.Response.Body.Position = 0;
        return await new StreamReader(http.Response.Body, Encoding.UTF8).ReadToEndAsync();
    }

    private static HealthReport Report(HealthStatus status, HealthReportEntry entry) => new(
        new Dictionary<string, HealthReportEntry> { ["database"] = entry },
        status,
        TimeSpan.FromMilliseconds(5));

    [Fact]
    public async Task Readiness_is_healthy_against_a_real_database()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var db = ContextFor(container.GetConnectionString());
        await db.Database.MigrateAsync();

        var result = await new DatabaseReadyCheck(db).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task Readiness_is_unhealthy_when_the_database_is_unreachable()
    {
        // Cổng 1 không có gì lắng nghe — CanConnectAsync phải trả false chứ không được ném ra ngoài.
        await using var db = ContextFor("Host=127.0.0.1;Port=1;Database=nope;Username=nope;Password=nope;Timeout=1");

        var result = await new DatabaseReadyCheck(db).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task Healthy_response_is_only_a_status()
    {
        var body = await WriteAsync(Report(
            HealthStatus.Healthy,
            new HealthReportEntry(HealthStatus.Healthy, null, TimeSpan.Zero, null, null)));

        Assert.Equal("{\"status\":\"Healthy\"}", body);
    }

    [Fact]
    public async Task Unhealthy_response_leaks_no_connection_string_or_stack_trace()
    {
        var secretish = new InvalidOperationException(
            "Host=db.internal;Username=admin;Password=examhub@123 failed at Npgsql.Internal");
        var body = await WriteAsync(Report(
            HealthStatus.Unhealthy,
            new HealthReportEntry(
                HealthStatus.Unhealthy,
                description: "Host=db.internal;Username=admin;Password=examhub@123",
                duration: TimeSpan.Zero,
                exception: secretish,
                data: new Dictionary<string, object> { ["connectionString"] = "Host=db.internal" })));

        Assert.Equal("{\"status\":\"Unhealthy\"}", body);
        foreach (var leak in new[] { "Password", "db.internal", "admin", "Npgsql", "connectionString" })
            Assert.DoesNotContain(leak, body, StringComparison.OrdinalIgnoreCase);
    }
}
