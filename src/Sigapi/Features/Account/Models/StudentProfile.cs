using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public sealed class StudentProfile
{
    [TextCasing(TextCasing.Title)]
    [ValueSelector("td.detalhes-usuario-nome > span")]
    public string Name { get; set; } = string.Empty;
    
    [ValueSelector("#detalhes-usuario tbody > tr:last-child > td:last-child")]
    public string Email { get; set; } = string.Empty;

    [ValueSelector("td.detalhes-usuario-matricula > small")]
    public string Enrollment { get; set; } = string.Empty;
    
    [ExistsSelector("a[href*='integralizacao']")]
    public bool IsProgramAvailable { get; set; }
}