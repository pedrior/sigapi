using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Faculties.Models;

[CollectionSelector("form > table.listagem:not(.unidade)")]
[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public sealed class FacultySummary
{
    private const string NamePattern =
        @"^\s*(?<name>[A-Z-]+)\s*-\s*(?<desc>.*?)\s*-\s*\k<name>\s*\.?$|^\s*(?<desc>.*?)\s*(?:\([A-Z-]+\))?\s*-\s*(?<name>[A-Z-]+)\s*\.?$";

    private const string PrefixesAndSuffixesPattern =
        @"^(?:[A-Za-z-]+\s+-\s+)|(?:\s+\([A-Za-z-]+\))?\s+-\s+[A-Za-z-]+\.?\s*$";

    [Regex(@"[?&]id=(\d+)")]
    [ValueSelector("a[class='iconeCentro']", Attribute = "href")]
    public string Id { get; set; } = string.Empty;

    [Regex(PrefixesAndSuffixesPattern, replacement: ""), Slug]
    [ValueSelector("a[class='nomeCentro']")]
    public string Slug { get; set; } = string.Empty;

    [Regex(NamePattern, group: "name"), TextCasing(TextCasing.Upper)]
    [ValueSelector("a[class='nomeCentro']")]
    public string Name { get; set; } = string.Empty;

    [Regex(PrefixesAndSuffixesPattern, replacement: ""), TextCasing(TextCasing.Title)]
    [ValueSelector("a[class='nomeCentro']")]
    public string LongName { get; set; } = string.Empty;
}