namespace Sigapi.Common.Scraping.Processing;

public class RegexAttribute : DataProcessorAttribute
{
    public RegexAttribute(string pattern, int order = 0) : base(RegexProcessor.Name, order)
    {
        Parameters = $"{RegexProcessor.PatternParameter}={pattern}";
    }
}