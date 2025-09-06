using Sigapi.Common.Scraping;
using Sigapi.Features.Account.Scraping.Processing;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record Enrollment
{
    private const string NumberKey = "identificador";
    
    [EnrollmentData]
    [ValueSelector("a", Attribute = "onclick")]
    public Dictionary<string, string> FormData { get; set; } = new();

    public string Number => FormData.GetValueOrDefault(NumberKey, string.Empty);
}