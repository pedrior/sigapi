namespace Sigapi.Common.Scraping.Networking.Sessions;

public interface ISessionStore
{
    Task SaveAsync(ISession session, CancellationToken cancellationToken = default);
    
    Task RefreshAsync(ISession session, CancellationToken cancellationToken = default);
    
    Task RevokeSession(string sessionId, CancellationToken cancellationToken = default);
    
    Task<bool> IsActiveAsync(string sessionId, CancellationToken cancellationToken = default);
    
    Task<ISession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default);
}