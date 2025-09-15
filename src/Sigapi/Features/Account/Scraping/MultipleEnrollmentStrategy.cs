using Sigapi.Common.Scraping.Networking;
using Sigapi.Features.Account.Errors;
using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public sealed class MultipleEnrollmentStrategy(
    IEnrollmentProvider enrollmentProvider,
    IEnrollmentSelector enrollmentSelector) : ILoginResponseStrategy
{
    public bool CanHandle(Page page)
    {
        var isEnrollmentSelector = page.Url.Contains("vinculos.jsf");
        var isMultipleEnrollment = page.Element.Query(EnrollmentSelector.EnrollmentSelectorLinkSelector) is not null;

        return isEnrollmentSelector || isMultipleEnrollment;
    }

    public async Task<ErrorOr<User>> HandleAsync(Page page,
        string username,
        string? enrollment = null,
        CancellationToken cancellationToken = default)
    {
        var enrollments = await enrollmentProvider.ListEnrollmentsAsync(page, cancellationToken);
        var enrollmentsArray = enrollments as Enrollment[] ?? enrollments.ToArray();

        return await FindEnrollmentToSelect(enrollmentsArray, enrollment)
            .ThenAsync(selected => enrollmentSelector.SelectAsync(
                username,
                selected,
                enrollmentsArray,
                cancellationToken));
    }

    private static ErrorOr<Enrollment> FindEnrollmentToSelect(Enrollment[] enrollments, string? targetEnrollment)
    {
        if (string.IsNullOrEmpty(targetEnrollment))
        {
            return enrollments.OrderByDescending(e => e.Number).First();
        }

        var enrollment = enrollments.SingleOrDefault(e => e.Number == targetEnrollment);
        return enrollment is not null
            ? enrollment
            : AccountErrors.EnrollmentMismatch;
    }
}