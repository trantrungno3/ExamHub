using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ExamHub.Core.Infrastructure.Caching;

/// <summary>
/// Dịch vụ cache dùng Redis (StackExchange.Redis qua IDistributedCache).
/// </summary>
public class RedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _defaultExpiry;

    /// <inheritdoc cref="RedisCacheService"/>
    public RedisCacheService(IDistributedCache cache, IConfiguration config)
    {
        _cache = cache;
        _defaultExpiry = TimeSpan.FromMinutes(
            int.TryParse(config["Redis:DefaultExpiryMinutes"], out var m) ? m : 10);
    }

    /// <summary>Lấy giá trị từ cache. Nếu không có, gọi factory để lấy và lưu lại.</summary>
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes is not null)
            return JsonSerializer.Deserialize<T>(bytes);

        var value = await factory();
        if (value is not null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry
            };
            await _cache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), options, ct);
        }
        return value;
    }

    /// <summary>Xóa cache theo key.</summary>
    public Task RemoveAsync(string key, CancellationToken ct = default)
        => _cache.RemoveAsync(key, ct);
}

