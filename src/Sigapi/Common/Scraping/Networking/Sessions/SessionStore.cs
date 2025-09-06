using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Hybrid;
using Sigapi.Common.Caching;

namespace Sigapi.Common.Scraping.Networking.Sessions;

public sealed class SessionStore(HybridCache cache, IDataProtectionProvider dataProtectionProvider) : ISessionStore
{
    public async Task SaveAsync(ISession session, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(session);
        var payload = Protect(session.Id, json);

        await cache.SetAsync(
            session.Id,
            payload,
            new HybridCacheEntryOptions
            {
                Expiration = session.GetRemainingLifetime()
            },
            cancellationToken: cancellationToken);
    }

    public async Task RevokeSession(string sessionId, CancellationToken cancellationToken = default) =>
        await cache.RemoveAsync(sessionId, cancellationToken: cancellationToken);

    public async Task<bool> IsActiveAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = await LoadAsync(sessionId, cancellationToken);
        return session is not null && !session.IsExpired;
    }

    public async Task<ISession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (await cache.GetAsync<string>(sessionId, cancellationToken: cancellationToken) is not { } payload)
        {
            return null;
        }

        try
        {
            var json = Unprotect(sessionId, payload);
            if (JsonSerializer.Deserialize<ISession>(json) is { } session)
            {
                return session;
            }
        }
        catch (CryptographicException)
        {
            // Ignored.
        }

        return null;
    }

    public async Task RefreshAsync(ISession session, CancellationToken cancellationToken = default)
    {
        if (await cache.ExistsAsync(session.Id, cancellationToken: cancellationToken))
        {
            await SaveAsync(session, cancellationToken);
        }
    }

    private string Protect(string sessionId, string data) =>
        CreateDataProtector(sessionId)
            .Protect(data, Session.Lifetime);

    private string Unprotect(string sessionId, string payload) =>
        CreateDataProtector(sessionId)
            .Unprotect(payload);

    private ITimeLimitedDataProtector CreateDataProtector(string sessionId) =>
        dataProtectionProvider.CreateProtector(purpose: sessionId)
            .ToTimeLimitedDataProtector();
}