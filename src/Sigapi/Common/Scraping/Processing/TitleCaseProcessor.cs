using System.Globalization;
using System.Text.RegularExpressions;

namespace Sigapi.Common.Scraping.Processing;

[UsedImplicitly]
public sealed partial class TitleCaseProcessor : IDataProcessor
{
    public const string Name = "title-case";

    private static readonly string[] LowercaseWords =
    [
        "a", "à", "às", "ao", "aos", "da", "das", "de", "do", "dos", "em",
        "no", "nos", "na", "nas", "e", "ou", "mas", "nem", "como", "por",
        "para", "com", "sem", "sob", "sobre", "até", "depois", "antes"
    ];

    public string UniqueName => Name;

    public object? Process(object? input, IDictionary<string, string>? parameters)
    {
        if (input is not string str || string.IsNullOrWhiteSpace(str))
        {
            return input;
        }

        var culture = CultureInfo.InvariantCulture;
        var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];

            word = ExtractPunctuation(word, out var prefix, out var suffix);

            // Process hyphenated sub-words
            var parts = word.Split('-');
            for (var j = 0; j < parts.Length; j++)
            {
                var part = parts[j];

                if (IsRomanNumeral(part))
                {
                    parts[j] = part.ToUpperInvariant();
                }
                else if (i > 0 && j is 0 && IsLowercaseWord(part))
                {
                    parts[j] = part.ToLowerInvariant();
                }
                else
                {
                    parts[j] = Capitalize(part, culture);
                }
            }

            var rebuilt = string.Join('-', parts);
            words[i] = prefix + rebuilt + suffix;
        }

        return string.Join(' ', words);
    }

    private static bool IsLowercaseWord(string word)
    {
        return Array.Exists(LowercaseWords, lowercaseWord =>
            lowercaseWord.Equals(word, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRomanNumeral(string word)
    {
        return RomanNumeralRegex()
            .IsMatch(word);
    }

    private static string Capitalize(string word, CultureInfo culture) => word.Length switch
    {
        0 => word,
        1 => culture.TextInfo.ToUpper(word),
        _ => culture.TextInfo.ToUpper(word[0]) + culture.TextInfo.ToLower(word[1..])
    };

    private static string ExtractPunctuation(string word, out string prefix, out string suffix)
    {
        var match = PunctuationRegex()
            .Match(word);

        if (match.Success)
        {
            prefix = match.Groups[1].Value;
            var middle = match.Groups[2].Value;
            suffix = match.Groups[3].Value;

            return middle;
        }

        prefix = "";
        suffix = "";

        return word;
    }

    [GeneratedRegex(
        "^(?=[MDCLXVI])M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$",
        RegexOptions.IgnoreCase)]
    private static partial Regex RomanNumeralRegex();

    [GeneratedRegex(@"^(\p{P}*)([\p{L}\p{N}\-]+)(\p{P}*)$")]
    private static partial Regex PunctuationRegex();
}