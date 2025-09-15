using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Networking.Sessions;

namespace Sigapi.Common.Scraping.Networking;

public sealed class PageFetcher : IPageFetcher
{
    private readonly HttpClient httpClient;
    private readonly ISessionFactory sessionFactory;
    private readonly IDocumentParser documentParser;

    public PageFetcher(HttpClient httpClient,
        ISessionFactory sessionFactory,
        IDocumentParser documentParser,
        IOptions<PageFetcherOptions> options)
    {
        this.httpClient = httpClient;
        this.sessionFactory = sessionFactory;
        this.documentParser = documentParser;

        InitializeHttpClient(options.Value);
    }

    public Task<Page> FetchAsync(string url,
        SessionPolicy sessionPolicy = SessionPolicy.Scoped,
        CancellationToken cancellationToken = default)
    {
        return FetchAsync(url, sessionPolicy, HttpMethod.Get, null, cancellationToken);
    }

    public Task<Page> FetchWithFormSubmissionAsync(string url,
        IDictionary<string, string> data,
        SessionPolicy sessionPolicy = SessionPolicy.Scoped,
        CancellationToken cancellationToken = default)
    {
        return FetchAsync(url,
            sessionPolicy,
            HttpMethod.Post,
            new FormUrlEncodedContent(data),
            cancellationToken);
    }

    private void InitializeHttpClient(PageFetcherOptions options)
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);
        foreach (var header in options.DefaultHeaders)
        {
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    private async Task<Page> FetchAsync(string url,
        SessionPolicy sessionPolicy,
        HttpMethod method,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = content
        };

        var session = await sessionFactory.Create(sessionPolicy);
        if (session is not null)
        {
            request.Options.Set(SessionHandler.SessionKey, session);
        }

        HttpResponseMessage? response = null;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = await documentParser.ParseAsync(html, cancellationToken);
            var absoluteUrl = response.RequestMessage?.RequestUri?.ToString() ?? "about:blank";

            return new Page(absoluteUrl, document, session);
        }
        catch (HttpRequestException ex)
        {
            throw new PageFetcherException($"Failed to fetch page at '{url}'", ex);
        }
        finally
        {
            response?.Dispose();
        }
    }
}