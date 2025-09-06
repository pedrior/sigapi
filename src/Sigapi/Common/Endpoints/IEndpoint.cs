namespace Sigapi.Common.Endpoints;

public interface IEndpoint
{
    void Map(IEndpointRouteBuilder route);
}