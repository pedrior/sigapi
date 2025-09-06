using System.Net;

namespace Sigapi.Common.Scraping.Networking.Sessions;

public sealed class Session : ISession
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(60);

    private static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(10);

    private static readonly Lock Sync = new();

    private readonly CookieContainer cookieContainer = new();

    public Session() => SetExpirationFromNow();

    internal Session(string id,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        IEnumerable<Cookie> cookies)
    {
        Id = id;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;

        foreach (var cookie in cookies)
        {
            cookieContainer.Add(cookie);
        }
    }

    public string Id { get; } = $"{Guid.CreateVersion7()}";

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; private set; }
    
    public bool IsExpired => DateTimeOffset.UtcNow + ClockSkew >= ExpiresAt;

    public async Task RefreshAsync(ISessionStore store)
    {
        SetExpirationFromNow();
        
        await store.RefreshAsync(this);
    }

    public void SetCookies(Uri target, IEnumerable<string> cookies)
    {
        lock (Sync)
        {
            foreach (var cookie in cookies)
            {
                cookieContainer.SetCookies(target, cookie);
            }
        }
    }

    public string GetCookies(Uri target)
    {
        lock (Sync)
        {
            return cookieContainer.GetCookieHeader(target);
        }
    }

    public IEnumerable<Cookie> ListCookies()
    {
        lock (Sync)
        {
            return cookieContainer.GetAllCookies();
        }
    }

    private void SetExpirationFromNow() => ExpiresAt = DateTimeOffset.UtcNow.Add(Lifetime);
}