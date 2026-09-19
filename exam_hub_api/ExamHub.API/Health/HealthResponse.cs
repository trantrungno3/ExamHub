using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ExamHub.API.Health;

/// <summary>
/// Response writer cho health endpoint. Body chỉ có đúng một trường status — mặc định của ASP.NET
/// in cả entry description/exception/data, tức là đường rò connection string và stack trace ra một
/// endpoint không cần đăng nhập.
/// </summary>
public static class HealthResponse
{
    /// <summary>Ghi <c>{"status":"..."}</c> và không gì khác.</summary>
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync($"{{\"status\":\"{report.Status}\"}}");
    }
}
