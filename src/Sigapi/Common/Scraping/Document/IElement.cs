namespace Sigapi.Common.Scraping.Document;

public interface IElement
{
    string? GetText();

    string? GetAttribute(string name);

    IElement? Query(string selector);

    IEnumerable<IElement> QueryAll(string selector);
}