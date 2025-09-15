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
public sealed class ProfileEndpoint : IEndpoint
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
        IRequestHandler<ProfileQuery, ProfileResponse> handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return handler.HandleAsync(new ProfileQuery(), cancellationToken)
            .Match(Results.Ok, errors => errors.ToProblemDetails());
    }
}

public sealed record ProfileQuery : IRequest;

[UsedImplicitly]
public sealed class ProfileQueryHandler(
    HybridCache cache,
    IPageFetcher pageFetcher,
    IScrapingService scrapingService,
    IUserContext userContext) : IRequestHandler<ProfileQuery, ProfileResponse>
{
    private static readonly HybridCacheEntryOptions CacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(30)
    };

    public async Task<ErrorOr<ProfileResponse>> HandleAsync(ProfileQuery query,
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
        var studentTask = ScrapeStudentAsync(cancellationToken);
        var detailsTask = ScrapeStudentDetailsAsync(cancellationToken);

        await Task.WhenAll(studentTask, detailsTask);

        var student = studentTask.Result;
        var details = detailsTask.Result;

        var program = student.IsProgramAvailable
            ? await ScrapeStudentProgramAsync(cancellationToken)
            : null;

        return new ProfileResponse
        {
            Name = program?.StudentName ?? student.Name,
            Email = student.Email,
            Username = userContext.Username,
            Enrollment = userContext.Enrollment,
            Enrollments = userContext.Enrollments,
            Photo = details.Photo,
            Biography = details.Biography,
            Interests = details.Interests,
            Curriculum = details.Curriculum,
            IsEnrolledInUndergraduateProgram = student.IsProgramAvailable
        };
    }

    private async Task<StudentProfile> ScrapeStudentAsync(CancellationToken cancellationToken)
    {
        var profilePage = await pageFetcher.FetchAsync(
            StudentPages.ProfileUrl,
            SessionPolicy.User,
            cancellationToken: cancellationToken);

        return scrapingService.Scrape<StudentProfile>(profilePage);
    }

    private async Task<StudentProfileDetails> ScrapeStudentDetailsAsync(CancellationToken cancellationToken)
    {
        var detailsPage = await pageFetcher.FetchAsync(
            StudentPages.DetailsUrl,
            SessionPolicy.User,
            cancellationToken: cancellationToken);

        return scrapingService.Scrape<StudentProfileDetails>(detailsPage);
    }

    private async Task<StudentProfileProgram> ScrapeStudentProgramAsync(CancellationToken cancellationToken)
    {
        var programPage = await pageFetcher.FetchAsync(
            StudentPages.ProgramUrl,
            SessionPolicy.User,
            cancellationToken: cancellationToken);

        return scrapingService.Scrape<StudentProfileProgram>(programPage);
    }
}