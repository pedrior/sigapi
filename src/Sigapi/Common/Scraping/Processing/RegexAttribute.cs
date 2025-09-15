namespace Sigapi.Common.Scraping.Processing;

public sealed class RegexAttribute : DataProcessorAttribute
{
    public RegexAttribute(string pattern, string? replacement = null, string? group = null, int order = 0) 
        : base(RegexProcessor.Name, order)
    {
        var parameters = $"{RegexProcessor.PatternParameter}={pattern}";
        if (replacement is not null)
        {
            parameters += $";{RegexProcessor.ReplacementParameter}={replacement}";
        }
        if (group is not null)
        {
            parameters += $";{RegexProcessor.GroupParameter}={group}";
        }
        
        Parameters = parameters;
    }
}