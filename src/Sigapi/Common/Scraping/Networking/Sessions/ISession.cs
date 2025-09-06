using System.Net;
using System.Text.Json.Serialization;

namespace Sigapi.Common.Scraping.Networking.Sessions;

[JsonConverter(typeof(SessionSerializer))]
public interface ISession
{
    string Id { get; }
    
    DateTimeOffset CreatedAt { get; }
    
    DateTimeOffset ExpiresAt { get; }

    bool IsExpired { get; }
    
    Task RefreshAsync(ISessionStore store);
    
    void SetCookies(Uri target, IEnumerable<string> cookies);
    
    string GetCookies(Uri target);

    IEnumerable<Cookie> ListCookies();
}