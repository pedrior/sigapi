using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Faculties.Models;

[CollectionSelector("table[class='listagem'] > tbody > tr")]
[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public sealed class FacultyGraduateProgram
{
    [Regex(@"[?&]id=(\d+)")]
    [ValueSelector("td > a", Attribute = "href")]
    public string Id { get; set; } = string.Empty;
    
    [Regex(@"(^[a-zA-Z\s]+-\s*)|\s*\([a-zA-Z]+\)$", replacement: ""), TextCasing(TextCasing.Title)]
    [ValueSelector("td > a")]
    public string Name { get; set; } = string.Empty;
}