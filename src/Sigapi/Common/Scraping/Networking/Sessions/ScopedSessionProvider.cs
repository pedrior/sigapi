namespace Sigapi.Common.Scraping.Networking.Sessions;

public sealed class ScopedSessionProvider : ISessionProvider
{
    private readonly ISession session = new Session();
    
    public ValueTask<ISession?> GetSessionAsync(CancellationToken cancellationToken = default) => new(session);
}