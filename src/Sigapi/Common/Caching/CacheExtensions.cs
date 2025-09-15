using Microsoft.Extensions.Caching.Hybrid;

namespace Sigapi.Common.Caching;

internal static class CacheExtensions
{
    private static readonly HybridCacheEntryOptions ReadOnlyCacheOptions = new()
    {
        Flags = HybridCacheEntryFlags.DisableUnderlyingData
    };

    public static async Task<T?> GetAsync<T>(this HybridCache cache,
        string key,
        CancellationToken cancellationToken = default)
    {
        var (_, value) = await cache.TryGetValueAsync<T>(key, cancellationToken);
        return value;
    }
    
    public static async Task<bool> ExistsAsync(this HybridCache cache,
        string key,
        CancellationToken cancellationToken = default)
    {
        var (exists, _) = await cache.TryGetValueAsync<object>(key, cancellationToken);
        return exists;
    }

    private static async Task<(bool exists, T value)> TryGetValueAsync<T>(this HybridCache cache,
        string key,
        CancellationToken cancellationToken = default)
    {
        var value = await cache.GetOrCreateAsync(
            key,
            async _ => await Task.FromResult<T>(default!),
            ReadOnlyCacheOptions,
            cancellationToken: cancellationToken);

        return (value is not null, value);
    }
}