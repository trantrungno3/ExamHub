using ExamHub.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ExamHub.API.Health;

/// <summary>
/// Readiness probe: API chỉ sẵn sàng nhận traffic khi nói được với PostgreSQL. Dùng
/// <c>CanConnectAsync</c> nên không cần thêm package health-check nào.
/// Redis/MinIO/Mongo do healthcheck native của Compose bao phủ, không lặp lại ở đây.
/// </summary>
public sealed class DatabaseReadyCheck(AppDbContext db) : IHealthCheck
{
    /// <summary>Trả Healthy khi mở được kết nối; mọi lỗi đều thành Unhealthy, không ném ra ngoài.</summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }
        catch (Exception)
        {
            // Không đính exception vào kết quả: HealthResponse không in nó ra, nhưng giữ ở đây thì
            // một response writer khác trong tương lai có thể vô tình rò connection string.
            return HealthCheckResult.Unhealthy();
        }
    }
}
