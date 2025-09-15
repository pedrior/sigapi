namespace Sigapi.Common.Scraping.Processing;

public sealed class TextCasingAttribute : DataProcessorAttribute
{
    public TextCasingAttribute(TextCasing casing, int order = 0) : base(TextCasingProcessor.Name, order)
    {
        Parameters = $"{TextCasingProcessor.CasingParameter}={casing}";
    }
}