using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public interface IEnrollmentProvider
{
    Task<IEnumerable<Enrollment>> ListEnrollmentsAsync(Page? enrollmentSelectorPage = null,
        CancellationToken cancellationToken = default);
}