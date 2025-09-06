using AngleSharp.Dom;

namespace Sigapi.Common.Scraping.Document;

public sealed class ElementSelector : IElementSelector
{
    public IElement? Select(IElement parent, string selector) => parent.QuerySelector(selector);

    public IEnumerable<IElement> SelectAll(IElement parent, string selector) => parent.QuerySelectorAll(selector);
}