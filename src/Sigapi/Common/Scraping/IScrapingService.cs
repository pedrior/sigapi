using AngleSharp.Dom;
using Sigapi.Common.Scraping.Networking;

namespace Sigapi.Common.Scraping;

public interface IScrapingService
{
    T Scrape<T>(Page root) where T : new();

    T Scrape<T>(IDocument root) where T : new();

    T Scrape<T>(IElement root) where T : new();
    
    IEnumerable<T> ScrapeMany<T>(Page root) where T : new();
    
    IEnumerable<T> ScrapeMany<T>(IDocument root) where T : new();
    
    IEnumerable<T> ScrapeMany<T>(IElement root) where T : new();
}