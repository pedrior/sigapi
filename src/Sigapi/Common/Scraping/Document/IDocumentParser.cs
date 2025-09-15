namespace Sigapi.Common.Scraping.Document;

public interface IDocumentParser
{
    Task<IElement> ParseAsync(string html, CancellationToken cancellationToken = default);
}