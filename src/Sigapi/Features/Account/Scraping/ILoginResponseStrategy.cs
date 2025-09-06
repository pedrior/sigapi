using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public interface ILoginResponseStrategy
{
    bool CanHandle(Page page);

    Task<ErrorOr<User>> HandleAsync(Page page,
        string username,
        string? enrollment = null,
        CancellationToken cancellationToken = default);
}