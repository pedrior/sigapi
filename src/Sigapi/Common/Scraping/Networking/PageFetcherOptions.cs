using System.ComponentModel.DataAnnotations;

namespace Sigapi.Common.Scraping.Networking;

public sealed record PageFetcherOptions
{
    [Url]
    public required string BaseUrl { get; init; }
    
    public required Dictionary<string, string> DefaultHeaders { get; init; }
}