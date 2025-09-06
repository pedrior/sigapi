namespace Sigapi.Common.Scraping;

public abstract class SelectorAttribute(string selector) : Attribute
{
    public string Selector { get; } = selector;

    public string? Attribute { get; set; }
    
    public bool IsRequired { get; set; } = true;
}