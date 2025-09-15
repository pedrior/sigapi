using Sigapi.Common.Scraping;

namespace Sigapi.Features.Account.Models;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public class StudentProfileDetails
{
    [ValueSelector("img[class*='fotoPerfil']", Attribute = "src", IsRequired = false)]
    public string? Photo { get; set; }
    
    [ValueSelector("textarea[id*='descricaoPessoal']", IsRequired = false)]
    public string? Biography { get; set; }
    
    [ValueSelector("textarea[id*='areasInteresse']", IsRequired = false)]
    public string? Interests { get; set; }
    
    [ValueSelector("input[id*='curriculoLattes']", IsRequired = false)]
    public string? Curriculum { get; set; }
}