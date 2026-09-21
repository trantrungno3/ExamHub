using System.Data.Common;
using System.Security.Claims;
using ExamHub.Core.Infrastructure.Persistence;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TVT.Core.Claims;

namespace ExamHub.Tests;

public class AuditExecuteUpdateTests
{
    [Fact]
    public async Task SetActive_stamps_current_editor_in_generated_update()
    {
        var command = new CaptureUpdateCommandInterceptor();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new SuppressConnectionInterceptor(), command)
            .Options;
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ConstClaim.UserName, "editor1")], "test"));
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };
        await using var db = new AppDbContext(options, accessor);

        var updated = await new GradeLevelRepository(db).SetActiveAsync(7, false);

        Assert.True(updated);
        Assert.Contains("modified =", command.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("modified_by =", command.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(command.ParameterValues, value => Equals(value, "editor1"));
    }
}

file sealed class SuppressConnectionInterceptor : DbConnectionInterceptor
{
    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result) => InterceptionResult.Suppress();

    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(InterceptionResult.Suppress());
}

file sealed class CaptureUpdateCommandInterceptor : DbCommandInterceptor
{
    public string CommandText { get; private set; } = string.Empty;
    public IReadOnlyList<object?> ParameterValues { get; private set; } = [];

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CommandText = command.CommandText;
        ParameterValues = command.Parameters.Cast<DbParameter>()
            .Select(x => x.Value)
            .ToList();
        return ValueTask.FromResult(InterceptionResult<int>.SuppressWithResult(1));
    }
}
