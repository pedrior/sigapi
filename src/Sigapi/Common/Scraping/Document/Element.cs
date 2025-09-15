using System.Text.RegularExpressions;

namespace Sigapi.Common.Scraping.Document;

public sealed partial class Element(AngleSharp.Dom.IElement element) : IElement
{
    public string? GetText()
    {
        var text = element.TextContent;
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return WhitespaceRegex()
            .Replace(text, " ")
            .Trim();
    }

    public string? GetAttribute(string name) => element.GetAttribute(name);

    public IElement? Query(string selector)
    {
        var result = element.QuerySelector(selector);
        return result is null
            ? null
            : new Element(result);
    }

    public IEnumerable<IElement> QueryAll(string selector)
    {
        var results = element.QuerySelectorAll(selector);
        return results.Select(e => new Element(e));
    }
    
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}