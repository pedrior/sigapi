using System.ComponentModel.DataAnnotations;

namespace Sigapi.Common.Auth.Tokens;

public sealed record SecurityTokenOptions
{
    public string? Issuer { get; init; }

    public string? Audience { get; init; }

    [Required]
    public string Key { get; init; } = null!;
}