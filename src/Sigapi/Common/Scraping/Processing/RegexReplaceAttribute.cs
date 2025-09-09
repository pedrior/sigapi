namespace Sigapi.Common.Scraping.Processing;

public sealed class RegexReplaceAttribute : DataProcessorAttribute
{
    public RegexReplaceAttribute(string pattern, string? value = null, int order = 0)
        : base(RegexReplaceProcessor.Name, order)
    {
        Parameters = $"{RegexReplaceProcessor.PatternParameter}={pattern};" +
                     $"{RegexReplaceProcessor.ValueParameter}={value}";
    }
}