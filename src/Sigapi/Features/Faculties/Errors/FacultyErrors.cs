namespace Sigapi.Features.Faculties.Errors;

public static class FacultyErrors
{
    public static Error NotFound(string idOrSlug) => Error.NotFound(
        code: "faculties.not_found",
        description: $"Faculty with ID or Slug '{idOrSlug}' was not found.");
}