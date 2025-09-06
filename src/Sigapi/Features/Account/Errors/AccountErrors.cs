namespace Sigapi.Features.Account.Errors;

public static class AccountErrors
{
    public static readonly Error CredentialsMismatch = Error.Unauthorized(
        code: "account.credentials_mismatch",
        description: "The provided username and password do not match any existing student account.");

    public static readonly Error EnrollmentMismatch = Error.Unauthorized(
        code: "account.enrollment_mismatch",
        description: "The provided enrollment does not match any of the student's enrollments.");
}