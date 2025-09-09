using System.Text.RegularExpressions;

namespace Sigapi.Common.Scraping.Processing;

public sealed class RegexReplaceProcessor : IDataProcessor
{
    public const string Name = "regex-replace";
    public const string PatternParameter = "pattern";
    public const string ValueParameter = "value";

    public string UniqueName => Name;

    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (input is not string str || string.IsNullOrWhiteSpace(str))
        {
            return null;
        }

        if (!parameters.TryGetValue(PatternParameter, out var pattern) || string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException($"Parameter '{PatternParameter}' must be a non-empty string.");
        }

        parameters.TryGetValue(ValueParameter, out var value);

        var result = Regex.Replace(
            str,
            pattern,
            value ?? string.Empty,
            RegexOptions.Compiled);

        return result == string.Empty ? null : result;
    }
}