using Sigapi.Features.Account.Models;

namespace Sigapi.Features.Account.Scraping;

public interface IEnrollmentSelector
{
    Task<ErrorOr<User>> SelectAsync(string username,
        Enrollment enrollment,
        IEnumerable<Enrollment> enrollments,
        CancellationToken cancellationToken = default);
}