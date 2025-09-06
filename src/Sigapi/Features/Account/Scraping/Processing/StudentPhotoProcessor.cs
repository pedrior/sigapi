using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Scraping.Processing;

[UsedImplicitly]
public sealed class StudentPhotoProcessor(IOptions<PageFetcherOptions> pageFetcherOptions) : IDataProcessor
{
    public const string Name = "student-photo";

    private const string PlaceholderUrl = "/sigaa/img/no_picture.png";

    public string UniqueName => Name;

    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (input is not string url || string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (url.Equals(PlaceholderUrl, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        var baseUrl = pageFetcherOptions.Value.BaseUrl;
        return new Uri(new Uri(baseUrl), url).ToString();
    }
}