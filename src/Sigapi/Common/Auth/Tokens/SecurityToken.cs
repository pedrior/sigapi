namespace Sigapi.Common.Auth.Tokens;

public readonly record struct SecurityToken(string Value, DateTimeOffset ExpiresAt);