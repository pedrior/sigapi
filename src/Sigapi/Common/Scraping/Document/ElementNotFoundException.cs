namespace Sigapi.Common.Scraping.Document;

public sealed class ElementNotFoundException(string message, Exception? inner = null)
    : ScrapingException(message, inner);