using Sigapi.Common.Security;

namespace Sigapi.Common.Scraping.Networking.Sessions;

public sealed class UserSessionProvider(IUserContext userContext, ISessionStore sessionStore) : ISessionProvider
{
    public async ValueTask<ISession?> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        var session = await sessionStore.LoadAsync(userContext.SessionId, cancellationToken);
        return session is { IsExpired: false }
            ? session
            : throw new SessionExpiredException("The session for the current user has expired.");
    }
}