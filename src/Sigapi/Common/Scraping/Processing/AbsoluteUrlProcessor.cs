using Sigapi.Common.Scraping.Networking;

namespace Sigapi.Common.Scraping.Processing;

[UsedImplicitly]
public sealed class AbsoluteUrlProcessor(IOptions<PageFetcherOptions> pageFetcherOptions) : IDataProcessor
{
    public const string Name = "absolute-url";
    
    public string UniqueName => Name;

    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (input is not string url || string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var baseUrl = pageFetcherOptions.Value.BaseUrl;
        return new Uri(new Uri(baseUrl), url).ToString();
    }
}