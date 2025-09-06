using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public sealed class LoginResponseHandler(IEnumerable<ILoginResponseStrategy> strategies) : ILoginResponseHandler
{
    public Task<ErrorOr<User>> HandleAsync(Page page,
        string username,
        string? enrollment = null,
        CancellationToken cancellationToken = default)
    {
        var strategy = strategies.FirstOrDefault(strategy => strategy.CanHandle(page));
        return strategy is null
            ? throw new ScrapingException($"No login response strategy found for page {page.Url}.")
            : strategy.HandleAsync(page, username, enrollment, cancellationToken);
    }
}