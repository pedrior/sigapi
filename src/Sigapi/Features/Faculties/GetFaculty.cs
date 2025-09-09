using Microsoft.Extensions.Caching.Hybrid;
using Sigapi.Common.Endpoints;
using Sigapi.Common.Errors;
using Sigapi.Common.Messaging;
using Sigapi.Common.RateLimiting;
using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Networking.Sessions;
using Sigapi.Contracts.Faculties;
using Sigapi.Features.Faculties.Errors;
using Sigapi.Features.Faculties.Models;
using Sigapi.Features.Faculties.Scraping;

namespace Sigapi.Features.Faculties;

[UsedImplicitly]
public sealed class GetFacultyEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder route)
    {
        route.MapGet("faculties/{idOrSlug}", GetFacultyAsync)
            .RequireRateLimiting(RateLimiterPolicies.Global)
            .WithSummary("Get faculty by ID or Slug")
            .WithDescription("Get detailed information about a specific faculty using its unique ID or slug.")
            .WithTags("Faculties")
            .Produces<FacultyResponse>()
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status499ClientClosedRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }

    private static Task<IResult> GetFacultyAsync(string idOrSlug,
        IRequestHandler<GetFacultyQuery, FacultyResponse> handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return handler.HandleAsync(new GetFacultyQuery(idOrSlug), cancellationToken)
            .Match(Results.Ok, e => e.ToProblemDetails());
    }
}

[UsedImplicitly]
public sealed record GetFacultyQuery(string IdOrSlug) : IRequest;

[UsedImplicitly]
public sealed class GetFacultyQueryHandler(
    HybridCache cache,
    IPageFetcher pageFetcher,
    IScrapingService scrapingService) : IRequestHandler<GetFacultyQuery, FacultyResponse>
{
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromDays(1)
    };

    public async Task<ErrorOr<FacultyResponse>> HandleAsync(GetFacultyQuery query,
        CancellationToken cancellationToken)
    {
        if (!Scraping.Faculties.TryGet(query.IdOrSlug, out var info))
        {
            return FacultyErrors.NotFound(query.IdOrSlug);
        }

        return await cache.GetOrCreateAsync(
            $"faculty:{info.Id}",
            token => GetFacultyAsync(info, token),
            CacheOptions,
            cancellationToken: cancellationToken);
    }

    private async ValueTask<FacultyResponse> GetFacultyAsync(FacultyInfo info, CancellationToken cancellationToken)
    {
        var facultyPageTask = FetchPageAsync(FacultyPages.GetFacultyUrl(info.Id), cancellationToken);
        var departmentsPageTask = FetchPageAsync(FacultyPages.GetDepartmentsUrl(info.Id), cancellationToken);
        var graduateProgramsPageTask = FetchPageAsync(
            FacultyPages.GetGraduateProgramsUrl(info.Id),
            cancellationToken);

        var undergraduateProgramsPageTask = FetchPageAsync(
            FacultyPages.GetUndergraduateCoursesUrl(info.Id),
            cancellationToken);

        await Task.WhenAll(
            facultyPageTask,
            departmentsPageTask,
            graduateProgramsPageTask,
            undergraduateProgramsPageTask);

        var facultyPage = facultyPageTask.Result;
        var departmentsPage = departmentsPageTask.Result;
        var graduateProgramsPage = graduateProgramsPageTask.Result;
        var undergraduateProgramsPage = undergraduateProgramsPageTask.Result;

        var faculty = scrapingService.Scrape<Faculty>(facultyPage);
        var departments = scrapingService.ScrapeMany<FacultyDepartment>(departmentsPage);
        var graduatePrograms = scrapingService.ScrapeMany<FacultyGraduateProgram>(graduateProgramsPage);
        var undergraduatePrograms = scrapingService.ScrapeMany<FacultyUndergraduateProgram>(undergraduateProgramsPage);

        return new FacultyResponse
        {
            Id = info.Id,
            Slug = info.Slug,
            Name = info.Name,
            LongName = faculty.LongName,
            Address = faculty.Address,
            Director = faculty.Director,
            Description = faculty.Description,
            LogoUrl = faculty.LogoUrl,
            Departments = departments.Select(d => new FacultyDepartmentResponse
            {
                Id = d.Id,
                Name = d.Name
            }),
            GraduatePrograms = graduatePrograms.Select(p => new FacultyGraduateProgramResponse
            {
                Id = p.Id,
                Name = p.Name
            }),
            UndergraduatePrograms = undergraduatePrograms.Select(p => new FacultyUndergraduateProgramResponse
            {
                Id = p.Id,
                Name = p.Name,
                City = p.City,
                Coordinator = p.Coordinator,
                IsOnsite = p.IsOnsite
            })
        };
    }

    private async Task<Page> FetchPageAsync(string url, CancellationToken cancellation) =>
        await pageFetcher.FetchAsync(url, SessionPolicy.None, cancellation);
}