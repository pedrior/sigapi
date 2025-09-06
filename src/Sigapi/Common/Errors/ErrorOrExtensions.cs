namespace Sigapi.Common.Errors;

internal static class ErrorOrExtensions
{
    public static ErrorOr<IEnumerable<T>> Consolidate<T>(this IEnumerable<ErrorOr<T>> results)
    {
        var resultsList = results as List<ErrorOr<T>> ?? results.ToList();
        var allErrors = resultsList.Where(r => r.IsError)
            .SelectMany(r => r.Errors)
            .ToArray();

        return allErrors.Length > 0
            ? allErrors
            : resultsList.Select(r => r.Value)
                .ToList();
    }

    public static IResult ToProblemDetails(this IList<Error> errors)
    {
        if (errors.Count is 0)
        {
            throw new ArgumentException("The errors list cannot be empty", nameof(errors));
        }

        int statusCode;
        Dictionary<string, string[]> errorsDictionary;

        if (errors.All(error => error.Type is ErrorType.Validation))
        {
            statusCode = MapErrorTypeToStatusCode(ErrorType.Validation);
            errorsDictionary = errors.GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g
                    .Select(e => e.Description)
                    .ToArray());
        }
        else
        {
            var firstError = errors[0];

            statusCode = MapErrorTypeToStatusCode(firstError.Type);
            errorsDictionary = new Dictionary<string, string[]>
            {
                { firstError.Code, [firstError.Description] }
            };
        }

        return Results.Problem(
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["error"] = errorsDictionary
            });
    }

    private static int MapErrorTypeToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError
    };
}