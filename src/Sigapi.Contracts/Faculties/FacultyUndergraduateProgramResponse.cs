namespace Sigapi.Contracts.Faculties;

public sealed record FacultyUndergraduateProgramResponse
{
    public string Id { get; init; } = string.Empty;
    
    public string Name { get; init; } = string.Empty;
    
    public string City { get; init; } = string.Empty;
    
    public string? Coordinator { get; init; }

    public bool IsOnsite { get; init; }
}