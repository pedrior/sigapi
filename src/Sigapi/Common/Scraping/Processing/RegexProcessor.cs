using System.Text.RegularExpressions;

namespace Sigapi.Common.Scraping.Processing;

[UsedImplicitly]
public sealed class RegexProcessor : IDataProcessor
{
    public const string Name = "regex";

    public const string PatternParameter = "pattern";
    public const string GroupParameter = "group";
    public const string ReplacementParameter = "replacement";

    private const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;

    public string UniqueName => Name;

    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (input is not string str || string.IsNullOrWhiteSpace(str) || parameters is null)
        {
            return input;
        }

        if (parameters.TryGetValue(PatternParameter, out var pattern) is not true ||
            string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException(
                $"The '{PatternParameter}' parameter is required and cannot be empty.",
                nameof(parameters));
        }

        if (parameters.TryGetValue(ReplacementParameter, out var replacement))
        {
            return Regex.Replace(str, pattern, replacement, DefaultRegexOptions);
        }

        // If not replacing, perform a match operation.
        var match = Regex.Match(str, pattern, DefaultRegexOptions);
        if (!match.Success)
        {
            return str; // Return the original string if no match is found.
        }

        // If a group is specified, return the value of that group.
        if (parameters.TryGetValue(GroupParameter, out var groupNameOrNumberStr))
        {
            var group = match.Groups[groupNameOrNumberStr];
            return group.Success
                ? group.Value
                : match.Value;
        }

        // Return the first capture group if it exists.
        return match.Groups.Count > 1
            ? match.Groups[1].Value
            : match.Value;
    }
}