using AngleSharp.Dom;
using Sigapi.Common.Scraping.Networking;

namespace Sigapi.Common.Scraping;

public interface IScrapingService
{
    T Scrape<T>(Page root) where T : class;

    T Scrape<T>(IDocument root) where T : class;

    T Scrape<T>(IElement root) where T : class;
}