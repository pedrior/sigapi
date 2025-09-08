using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Processing;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public class StudentProfileDetails
{
    [AbsoluteUrl]
    [ValueSelector("img[class*='fotoPerfil']", Attribute = "src")]
    public string? Photo { get; set; }

    [NullIfEmpty, NormalizeWhitespace]
    [ValueSelector("textarea[id*='descricaoPessoal']")]
    public string? Biography { get; set; }

    [NullIfEmpty, NormalizeWhitespace]
    [ValueSelector("textarea[id*='areasInteresse']")]
    public string? Interests { get; set; }

    [NullIfEmpty, NormalizeWhitespace]
    [ValueSelector("input[id*='curriculoLattes']")]
    public string? Curriculum { get; set; }
}