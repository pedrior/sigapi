using Microsoft.Extensions.Caching.Hybrid;
using Sigapi.Common.Endpoints;
using Sigapi.Common.Errors;
using Sigapi.Common.Messaging;
using Sigapi.Common.RateLimiting;
using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Networking.Sessions;
using Sigapi.Common.Security;
using Sigapi.Contracts.Account;
using Sigapi.Features.Account.Models;
using Sigapi.Features.Account.Scraping;
using Sigapi.OpenApi.Extensions;

namespace Sigapi.Features.Account;

[UsedImplicitly]
public sealed class GetProfileEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder route)
    {
        route.MapGet("account/profile", GetProfileAsync)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimiterPolicies.Global)
            .WithSummary("Get Profile")
            .WithDescription("Retrieves the profile information of the authenticated student.")
            .WithTags("Account")
            .WithRequireAuthentication()
            .Produces<ProfileResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status499ClientClosedRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }

    private static Task<IResult> GetProfileAsync(
        IRequestHandler<GetProfileQuery, ProfileResponse> handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return handler.HandleAsync(new GetProfileQuery(), cancellationToken)
            .Match(Results.Ok, errors => errors.ToProblemDetails());
    }
}

public sealed record GetProfileQuery : IRequest;

[UsedImplicitly]
public sealed class GetProfileQueryHandler(
    HybridCache cache,
    IPageFetcher pageFetcher,
    IScrapingService scrapingService,
    IUserContext userContext) : IRequestHandler<GetProfileQuery, ProfileResponse>
{
    private static readonly HybridCacheEntryOptions CacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(30)
    };

    public async Task<ErrorOr<ProfileResponse>> HandleAsync(GetProfileQuery query,
        CancellationToken cancellationToken)
    {
        var response = await cache.GetOrCreateAsync(
            key: $"profile:{userContext.Username}:{userContext.Enrollment}",
            factory: GetProfileAsync,
            CacheEntryOptions,
            cancellationToken: cancellationToken);

        return response;
    }

    private async ValueTask<ProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var page = await pageFetcher.FetchAsync(
            StudentPages.ProfileUrl,
            SessionPolicy.User,
            cancellationToken: cancellationToken);

        var profile = scrapingService.Scrape<StudentProfile>(page);
        var program = profile.IsProgramAvailable
            ? await GetStudentProgramAsync(cancellationToken)
            : null;

        return new ProfileResponse
        {
            Name = program?.StudentName ?? profile.Name,
            Email = profile.Email,
            Username = userContext.Username,
            Enrollment = userContext.Enrollment,
            Enrollments = userContext.Enrollments,
            Photo = profile.Photo,
            Biography = profile.Biography,
            Interests = profile.Interests,
            Curriculum = profile.Curriculum,
            IsEnrolledInUndergraduateProgram = profile.IsProgramAvailable
        };
    }

    private async Task<StudentProfileProgram> GetStudentProgramAsync(CancellationToken cancellationToken)
    {
        var page = await pageFetcher.FetchAsync(
            StudentPages.ProgramUrl,
            SessionPolicy.User,
            cancellationToken: cancellationToken);

        return scrapingService.Scrape<StudentProfileProgram>(page);
    }
}