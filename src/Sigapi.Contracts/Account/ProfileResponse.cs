namespace Sigapi.Contracts.Account;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public sealed record ProfileResponse
{
    public string Name { get; init; } = string.Empty;
    
    public string Email { get; init; } = string.Empty;
    
    public string Username { get; init; } = string.Empty;
    
    public string Enrollment { get; init; } = string.Empty;

    public IEnumerable<string> Enrollments { get; init; } = [];
    
    public string? Photo { get; init; }
    
    public string? Biography { get; init; }
    
    public string? Interests { get; init; }
    
    public string? Curriculum { get; init; }
    
    public bool IsEnrolledInUndergraduateProgram { get; init; }
}