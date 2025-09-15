using AngleSharp.Html.Parser;

namespace Sigapi.Common.Scraping.Document;

public sealed class DocumentParser : IDocumentParser
{
    private readonly HtmlParser parser = new(new HtmlParserOptions
    {
        SkipCDATA = true,
        SkipComments = true,
        SkipScriptText = true,
        IsStrictMode = false,
        IsPreservingAttributeNames = true,
        DisableElementPositionTracking = true
    });

    public async Task<IElement> ParseAsync(string html, CancellationToken cancellationToken = default)
    {
        var document = await parser.ParseDocumentAsync(html, cancellationToken);
        return new Element(document.DocumentElement);
    }
}