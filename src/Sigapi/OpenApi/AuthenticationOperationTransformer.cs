using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Sigapi.OpenApi.Extensions;

namespace Sigapi.OpenApi;

public sealed class AuthenticationOperationTransformer : IOpenApiOperationTransformer
{
    public const string ExtensionName = "x-authentication";

    public Task TransformAsync(OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var requireAuthentication = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<RequireAuthenticationAttribute>()
            .FirstOrDefault() is not null;

        operation.Extensions.TryAdd(ExtensionName, new OpenApiBoolean(requireAuthentication));

        return Task.CompletedTask;
    }
}