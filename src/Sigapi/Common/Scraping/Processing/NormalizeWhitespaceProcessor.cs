using System.Text.RegularExpressions;

namespace Sigapi.Common.Scraping.Processing;

[UsedImplicitly]
public sealed partial class NormalizeWhitespaceProcessor : IDataProcessor
{
    public const string Name = "normalize-whitespace";

    public string UniqueName => Name;
    
    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (input is not string str || string.IsNullOrWhiteSpace(str))
        {
            return input;
        }

        return WhitespaceRegex()
            .Replace(str, " ")
            .Trim();
    }
    
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}