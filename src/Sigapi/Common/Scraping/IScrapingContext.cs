using Sigapi.Common.Scraping.Document;

namespace Sigapi.Common.Scraping;

public interface IScrapingContext
{
    object ScrapeObject(Type type, IElement parent);
    
    IEnumerable<object> ScrapeObjectCollection(Type itemType, IElement parent);
}