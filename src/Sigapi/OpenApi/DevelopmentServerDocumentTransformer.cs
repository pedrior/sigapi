using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Sigapi.OpenApi;

public sealed class DevelopmentServerDocumentTransformer(IHttpContextAccessor httpContextAccessor)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext is not { } httpContext)
        {
            return Task.CompletedTask;
        }

        // Build the development server URL using the current scheme, host, and path base.
        // This ensures the OpenAPI-based UI is served from the correct location, resolving
        // issues where it incorrectly points to localhost, especially when we are behind a
        // reverse proxy.
        var request = httpContext.Request;
        var serverUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

        document.Servers.Clear(); // We don't care.

        document.Servers.Add(new OpenApiServer
        {
            Url = serverUrl,
            Description = "Development server"
        });

        return Task.CompletedTask;
    }
}