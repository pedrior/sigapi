namespace Sigapi.Common.Scraping.Networking.Sessions;

public static class SessionExtensions
{
    public static TimeSpan GetRemainingLifetime(this ISession session) =>
        session.ExpiresAt - DateTimeOffset.UtcNow;
}