using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Errors;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public sealed class CredentialsMismatchStrategy : ILoginResponseStrategy
{
    public bool CanHandle(Page page) => page.Url.Contains("logon.jsf");

    public Task<ErrorOr<User>> HandleAsync(Page page,
        string username,
        string? enrollment = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ErrorOr<User>>(AccountErrors.CredentialsMismatch);
    }
}