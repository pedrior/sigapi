using Microsoft.AspNetCore.Diagnostics;
using Sigapi.Common.Scraping.Networking;

namespace Sigapi.Middlewares;

public sealed class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = GetStatusCodeForException(exception);
        await Results.Problem(statusCode: statusCode, title: title)
            .ExecuteAsync(context);

        return true;
    }

    private static (int code, string? title) GetStatusCodeForException(Exception exception) => exception switch
    {
        UnauthorizedAccessException => GetStatus401Unauthorized(),
        OperationCanceledException => GetStatus499ClientClosedRequest(),
        PageFetcherException => GetStatus503ServiceUnavailable(),
        _ => GetStatus500InternalServerError()
    };

    private static (int code, string title) GetStatus401Unauthorized() => (StatusCodes.Status401Unauthorized,
        "You are not authorized to access this resource.");

    private static (int, string) GetStatus499ClientClosedRequest() => (StatusCodes.Status499ClientClosedRequest,
        "The client closed the connection before the server could complete the response.");

    private static (int, string) GetStatus500InternalServerError() => (StatusCodes.Status500InternalServerError,
        "We ran into an unexpected problem while processing your request.");

    private static (int code, string? title) GetStatus503ServiceUnavailable() => (
        StatusCodes.Status503ServiceUnavailable,
        "The server is currently unable to handle the request. Please try again later.");
}