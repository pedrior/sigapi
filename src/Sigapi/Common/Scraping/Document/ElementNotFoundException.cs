namespace Sigapi.Common.Scraping.Document;

public sealed class ElementNotFoundException : ScrapingException
{
    public ElementNotFoundException(string selector, string property) :
        this("Required element not found.", selector, property)
    {
    }

    public ElementNotFoundException(string message, string selector, string property) : base(message)
    {
        Selector = selector;
        Property = property;
    }

    public string Selector { get; }
    
    public string Property { get; }
}