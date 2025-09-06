namespace Sigapi.Common.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder builder, IServiceProvider services)
    {
        foreach (var endpoint in services.GetRequiredService<IEnumerable<IEndpoint>>())
        {
            endpoint.Map(builder);
        }
    }
}