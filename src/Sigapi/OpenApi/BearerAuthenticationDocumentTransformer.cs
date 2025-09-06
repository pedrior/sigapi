using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Sigapi.OpenApi;

public sealed class BearerAuthenticationDocumentTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    private const string SchemeName = "Bearer";
    
    public async Task TransformAsync(OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(scheme => scheme.Name is SchemeName))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                [SchemeName] = new()
                {
                    Scheme = SchemeName,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                }
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                var requiredAuthenticationExtension = operation.Value.Extensions.FirstOrDefault(pair =>
                    pair.Key is AuthenticationOperationTransformer.ExtensionName).Value as OpenApiBoolean;

                if (requiredAuthenticationExtension is null or { Value: false })
                {
                    continue;
                }

                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SchemeName
                        }
                    }] = []
                });
            }
        }
    }
}