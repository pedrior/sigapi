using Sigapi.Common.Scraping.Document;

namespace Sigapi.Common.Scraping;

public interface IScrapingService
{
    T Scrape<T>(IElement element) where T : class;
}