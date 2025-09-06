namespace Sigapi.OpenApi.Extensions;

public static class EndpointBuilderExtensions
{
    public static TBuilder WithRequireAuthentication<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder => builder.WithMetadata(new RequireAuthenticationAttribute());
}