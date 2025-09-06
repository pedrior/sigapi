using System.Text.RegularExpressions;
using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Scraping.Processing;

[UsedImplicitly]
public sealed partial class EnrollmentDataProcessor : IDataProcessor
{
    public const string Name = "enrollment-data";

    public string UniqueName => Name;

    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (input is not string str || string.IsNullOrEmpty(str))
        {
            return null;
        }

        var matches = DataRegex()
            .Matches(str);

        if (matches.Count is 0)
        {
            throw new ScrapingException($"Failed to parse enrollment data from string: {str}.");
        }

        var data = matches.ToDictionary(
            m => m.Groups[1].Value,
            m => m.Groups[2].Value);

        return data;
    }

    [GeneratedRegex(@"'([^']+)':\s*'([^']*)'")]
    private static partial Regex DataRegex();
}