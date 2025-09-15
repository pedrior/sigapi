namespace Sigapi.Contracts.Faculties;

public sealed record FacultySummaryResponse
{
    public string Id { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string LongName { get; init; } = string.Empty;
}