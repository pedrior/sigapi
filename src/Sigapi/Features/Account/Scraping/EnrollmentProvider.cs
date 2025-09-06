using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public sealed class EnrollmentProvider(
    IPageFetcher pageFetcher,
    IScrapingService scrapingService) : IEnrollmentProvider
{
    public async Task<IEnumerable<Enrollment>> ListEnrollmentsAsync(Page? enrollmentSelectorPage = null,
        CancellationToken cancellationToken = default)
    {
        enrollmentSelectorPage = await GetEnrollmentSelectorPageIfNeededAsync(null, cancellationToken);
        var enrollmentDetails = scrapingService.Scrape<EnrollmentListDetails>(enrollmentSelectorPage);

        return enrollmentDetails.Active
            .Concat(enrollmentDetails.Inactive)
            .Select(ApplyCommonEnrollmentFormData);

        Enrollment ApplyCommonEnrollmentFormData(Enrollment enrollment)
        {
            foreach (var (name, value) in enrollmentDetails.CommonFormData)
            {
                enrollment.FormData[name] = value;
            }

            return enrollment;
        }
    }

    private async Task<Page> GetEnrollmentSelectorPageIfNeededAsync(Page? enrollmentSelectorPage,
        CancellationToken cancellationToken)
    {
        // If we're already on the enrollment selector page, we don't need to fetch it again.
        const string targetPage = AccountPages.EnrollmentSelector;
        return enrollmentSelectorPage?.Url.EndsWith(targetPage) is true
            ? enrollmentSelectorPage
            : await pageFetcher.FetchAsync(targetPage, cancellationToken: cancellationToken);
    }
}