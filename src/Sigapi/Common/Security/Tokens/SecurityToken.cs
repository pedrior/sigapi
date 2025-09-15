namespace Sigapi.Common.Security.Tokens;

public readonly record struct SecurityToken(string Value, DateTimeOffset ExpiresAt);