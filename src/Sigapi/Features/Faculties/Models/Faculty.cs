using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Faculties.Models;

[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public sealed class Faculty
{
    [RegexReplace(@"(^[a-zA-Z\s]+-\s*)|\s*\([a-zA-Z]+\)$"), TitleCase]
    [ValueSelector("#colDirTop > h2")]
    public string LongName { get; set; } = string.Empty;

    [RegexReplace(@"(?i)\bnão informado\b")]
    [ValueSelector("#colDirCorpo > dl:not(.apresentacao) > dd:nth-child(6)", IsRequired = false)]
    public string? Address { get; set; }
    
    [RegexReplace(@"(?i)\bnão informado\b")]
    [ValueSelector("#colDirCorpo > dl:not(.apresentacao) > dd:nth-child(2)", IsRequired = false)]
    public string? Director { get; set; }
    
    [RegexReplace(@"(?i)\bnão informado\b")]
    [ValueSelector("#colDirCorpo > dl.apresentacao", IsRequired = false)]
    public string? Description { get; set; }
    
    [ValueSelector("#logo > p > span > a > img, #logo > p > span > img", Attribute = "src")]
    public string LogoUrl { get; set; } = null!;
}