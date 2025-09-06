namespace Sigapi.Common.Scraping.Networking.Sessions;

public interface ISessionProvider
{
    ValueTask<ISession?> GetSessionAsync(CancellationToken cancellationToken = default);
}