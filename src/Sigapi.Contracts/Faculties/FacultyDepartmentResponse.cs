namespace Sigapi.Contracts.Faculties;

public sealed record FacultyDepartmentResponse
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}