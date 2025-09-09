using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Sigapi.Features.Faculties.Scraping;

public static class Faculties
{
    private static readonly ImmutableHashSet<FacultyInfo> All =
    [
        new("1632", "CCAR", "centro-de-ciencias-aplicadas-e-educacao"),
        new("1614", "CCM", "centro-de-ciencias-medicas"),
        new("2564", "CCTA", "centro-de-comunicacao-turismo-e-artes"),
        new("1852", "CEAR", "centro-de-energias-e-alternativas-e-renovaveis"),
        new("1383", "CE", "centro-de-educacao"),
        new("1860", "CBIOTEC", "centro-de-biotecnologia"),
        new("1466", "CCA", "centro-de-ciencias-agrarias"),
        new("1357", "CCS", "centro-de-ciencias-da-saude"),
        new("1333", "CCEN", "centro-de-ciencias-exatas-e-da-natureza"),
        new("1345", "CCHLA", "centro-de-ciencias-humanas-letras-e-artes"),
        new("1472", "CCHSA", "centro-de-ciencias-humanas-sociais-e-agrarias"),
        new("1388", "CCJ", "centro-de-ciencias-juridicas"),
        new("1327", "CCSA", "centro-de-sociais-e-aplicadas"),
        new("1856", "CI", "centro-de-informatica"),
        new("3687", "CPT-ETS", "centro-profissional-e-tecnologico-escola-tecnica-de-saude"),
        new("1374", "CT", "centro-de-tecnologia"),
        new("1580", "CTDT", "centro-de-tecnologia-e-desenvolvimento-regional")
    ];

    public static bool TryGet(string idOrSlug, [NotNullWhen(true)] out FacultyInfo? info)
    {
        info = All.FirstOrDefault(x => x.Id == idOrSlug || x.Slug == idOrSlug);
        return info is not null;
    }
}