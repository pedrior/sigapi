namespace Sigapi.Contracts.Account;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public sealed record LoginResponse
{
    public string Token { get; init; } = null!;
    
    public DateTimeOffset ExpiresAt { get; init; }
}