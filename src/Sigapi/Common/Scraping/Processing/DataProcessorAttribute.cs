namespace Sigapi.Common.Scraping.Processing;

[AttributeUsage(AttributeTargets.Property)]
public class DataProcessorAttribute(string name, int order = 0) : Attribute
{
    public string Name => name;

    public int Order => order;

    public string? Parameters { get; set; }
}