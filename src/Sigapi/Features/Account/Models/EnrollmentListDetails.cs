using Sigapi.Common.Scraping;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public sealed class EnrollmentListDetails
{
    [AttributeSelector("form[action*='vinculos.jsf'] input", Key = "name", Value = "value")]
    public Dictionary<string, string> CommonFormData { get; set; } = new();

    [CollectionSelector("section.subformulario:not(#lista-vinculos-inativos) > div")]
    public IEnumerable<Enrollment> Active { get; set; } = [];

    [CollectionSelector("#lista-vinculos-inativos > div")]
    public IEnumerable<Enrollment> Inactive { get; set; } = [];
}