namespace Sigapi.Common.Security.Tokens;

public interface ISecurityTokenProvider
{
    (string token, DateTimeOffset expiresAt) CreateToken(IDictionary<string, object> claims);
}