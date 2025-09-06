using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Errors;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public sealed class SingleEnrollmentStrategy(IEnrollmentProvider enrollmentProvider) : ILoginResponseStrategy
{
    public bool CanHandle(Page page)
    {
        var isStudentPage = page.Url.Contains("discente.jsf");
        var isSingleEnrollment = page.Document.QuerySelector(
            EnrollmentSelector.EnrollmentSelectorLinkSelector) is null;

        return isStudentPage && isSingleEnrollment;
    }

    public async Task<ErrorOr<User>> HandleAsync(Page page,
        string username,
        string? enrollment = null,
        CancellationToken cancellationToken = default)
    {
        var enrollments = await enrollmentProvider.ListEnrollmentsAsync(
            enrollmentSelectorPage: null,
            cancellationToken);

        var foundEnrollment = enrollments.First();

        // We know that the user has only one enrollment, but we still return an error
        // if the enrollment provided by the user doesn't match the one we found.
        if (enrollment is not null && enrollment != foundEnrollment.Number)
        {
            return AccountErrors.EnrollmentMismatch;
        }

        return new User(username, foundEnrollment);
    }
}