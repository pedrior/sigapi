using Sigapi.Common.Endpoints;
using Sigapi.Common.Errors;
using Sigapi.Common.Messaging;
using Sigapi.Common.RateLimiting;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Networking.Sessions;
using Sigapi.Common.Security;
using Sigapi.Features.Account.Scraping;
using Sigapi.OpenApi.Extensions;

namespace Sigapi.Features.Account;

[UsedImplicitly]
public sealed class LogoutEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder route)
    {
        route.MapPost("account/logout", LogoutAsync)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimiterPolicies.Authentication)
            .WithSummary("Logout")
            .WithDescription("Logs out the authenticated student, invalidating their session and access token.")
            .WithTags("Account")
            .WithRequireAuthentication()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }

    private static Task<IResult> LogoutAsync(
        IRequestHandler<LogoutCommand, Success> handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return handler.HandleAsync(new LogoutCommand(), cancellationToken)
            .Match(_ => Results.NoContent(), errors => errors.ToProblemDetails());
    }
}

public sealed record LogoutCommand : IRequest;

[UsedImplicitly]
public sealed class LogoutCommandHandler(
    IPageFetcher pageFetcher,
    IUserContext userContext,
    ISessionStore sessionStore) : IRequestHandler<LogoutCommand, Success>
{
    public async Task<ErrorOr<Success>> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        await pageFetcher.FetchAsync(AccountPages.Logout, SessionPolicy.User, cancellationToken);
        await sessionStore.RevokeSession(userContext.SessionId, cancellationToken);

        return Result.Success;
    }
}