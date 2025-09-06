using AngleSharp.Dom;

namespace Sigapi.Common.Scraping.Document;

public interface IElementSelector
{
    IElement? Select(IElement parent, string selector);
    
    IEnumerable<IElement> SelectAll(IElement parent, string selector);
}