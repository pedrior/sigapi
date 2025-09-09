namespace Sigapi.Contracts.Faculties;

public sealed record FacultyResponse
{
    public string Id { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string LongName { get; init; } = string.Empty;

    public string? Address { get; init; }

    public string? Director { get; init; }

    public string? Description { get; init; }

    public string? LogoUrl { get; init; }

    public IEnumerable<FacultyDepartmentResponse> Departments { get; init; } = [];
    
    public IEnumerable<FacultyGraduateProgramResponse> GraduatePrograms { get; init; } = [];
    
    public IEnumerable<FacultyUndergraduateProgramResponse> UndergraduatePrograms { get; init; } = [];
}