using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public sealed class EnrollmentSelector(IPageFetcher pageFetcher) : IEnrollmentSelector
{
    public const string EnrollmentSelectorLinkSelector = "a[href*='vinculos/listar']";
    
    public async Task<ErrorOr<User>> SelectAsync(string username,
        Enrollment enrollment,
        IEnumerable<Enrollment> enrollments,
        CancellationToken cancellationToken = default)
    {
        var response = await pageFetcher.FetchWithFormSubmissionAsync(
            AccountPages.EnrollmentSelector,
            enrollment.FormData,
            cancellationToken: cancellationToken);

        return response.Url.Contains("discente.jsf")
            ? new User(username, enrollment, enrollments)
            : throw new ScrapingException(
                $"Unexpected response after submitting enrollment selector form: {response.Url}.");
    }
}