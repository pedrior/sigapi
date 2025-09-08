using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public class StudentProfileDetails
{
    [AbsoluteUrl]
    [ValueSelector("img[class*='fotoPerfil']", Attribute = "src", IsRequired = false)]
    public string? Photo { get; set; }

    [NullIfEmpty, NormalizeWhitespace]
    [ValueSelector("textarea[id*='descricaoPessoal']", IsRequired = false)]
    public string? Biography { get; set; }

    [NullIfEmpty, NormalizeWhitespace]
    [ValueSelector("textarea[id*='areasInteresse']", IsRequired = false)]
    public string? Interests { get; set; }

    [NullIfEmpty, NormalizeWhitespace]
    [ValueSelector("input[id*='curriculoLattes']", IsRequired = false)]
    public string? Curriculum { get; set; }
}