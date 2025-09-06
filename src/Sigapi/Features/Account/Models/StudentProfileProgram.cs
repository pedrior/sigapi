using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public sealed class StudentProfileProgram
{
    [TitleCase]
    [ValueSelector(".visualizacao > tbody > tr:nth-child(2) > td")]
    public string StudentName { get; set; } = string.Empty;
}