using System.Text.RegularExpressions;

namespace Sigapi.Common.Scraping.Processing;

public sealed class RegexProcessor : IDataProcessor
{
    public const string Name = "regex";
    public const string PatternParameter = "pattern";

    public string UniqueName => Name;

    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (parameters?.TryGetValue(PatternParameter, out var pattern) is not true)
        {
            throw new ArgumentException($"The '{PatternParameter}' parameter is required.", nameof(parameters));
        }

        if (input is not string str || string.IsNullOrWhiteSpace(str))
        {
            return null;
        }

        var match = Regex.Match(str, pattern, RegexOptions.Compiled);
        return match is { Success: true, Groups.Count: > 1 }
            ? match.Groups[1].Value
            : null;
    }
}