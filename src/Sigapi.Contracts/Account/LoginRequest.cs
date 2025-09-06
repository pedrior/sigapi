namespace Sigapi.Contracts.Account;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public sealed record LoginRequest
{
    public string Username { get; init; } = null!;
    
    public string Password { get; init; } = null!;
    
    public string? Enrollment { get; init; }
}