namespace Sigapi.Common.Scraping;

public sealed class AttributeSelectorAttribute(string selector) : SelectorAttribute(selector)
{
    public required string Key { get; init; }
    
    public required string Value { get; set; }
}