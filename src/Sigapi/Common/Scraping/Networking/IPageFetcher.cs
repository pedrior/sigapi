using Sigapi.Common.Scraping.Networking.Sessions;

namespace Sigapi.Common.Scraping.Networking;

public interface IPageFetcher
{
    Task<Page> FetchAsync(string url, 
        SessionPolicy sessionPolicy = SessionPolicy.Scoped, 
        CancellationToken cancellationToken = default);

    Task<Page> FetchWithFormSubmissionAsync(string url, 
        IDictionary<string, string> data,
        SessionPolicy sessionPolicy = SessionPolicy.Scoped, 
        CancellationToken cancellationToken = default);
}