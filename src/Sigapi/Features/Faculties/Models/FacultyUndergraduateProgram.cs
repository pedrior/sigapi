using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Faculties.Models;

[CollectionSelector("table[class='listagem'] > tbody > tr")]
[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public sealed class FacultyUndergraduateProgram
{
    [Regex(@"[?&]id=(\d+)")]
    [ValueSelector("td:last-child > a", Attribute = "href")]
    public string Id { get; set; } = string.Empty;

    [Regex(@"^(.*?)(?=\/)"), TextCasing(TextCasing.Title)]
    [ValueSelector("td")]
    public string Name { get; set; } = string.Empty;

    [TextCasing(TextCasing.Title)]
    [ValueSelector("td:nth-child(2)")]
    public string City { get; set; } = string.Empty;

    [TextCasing(TextCasing.Title)]
    [ValueSelector("td:nth-child(4)")]
    public string? Coordinator { get; set; }

    [ExistsSelector("td:nth-child(3):contains('Presencial')")]
    public bool IsOnsite { get; set; }
}