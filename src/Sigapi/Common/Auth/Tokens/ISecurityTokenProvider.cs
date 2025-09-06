namespace Sigapi.Common.Auth.Tokens;

public interface ISecurityTokenProvider
{
    (string token, DateTimeOffset expiresAt) CreateToken(IDictionary<string, object> claims);
}