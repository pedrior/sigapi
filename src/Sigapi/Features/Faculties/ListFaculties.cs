using Sigapi.Common.Endpoints;
using Sigapi.Common.Errors;
using Sigapi.Common.Messaging;
using Sigapi.Common.RateLimiting;
using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Networking.Sessions;
using Sigapi.Contracts.Faculties;
using Sigapi.Features.Faculties.Models;
using Sigapi.Features.Faculties.Scraping;

namespace Sigapi.Features.Faculties;

[UsedImplicitly]
public sealed class ListFacultiesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder route)
    {
        route.MapGet("faculties", ListFacultiesAsync)
            .RequireRateLimiting(RateLimiterPolicies.Global)
            .WithSummary("List all faculties")
            .WithDescription("Retrieves a list of all faculties available in the system.")
            .WithTags("Faculties")
            .Produces<IEnumerable<FacultySummaryResponse>>()
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status499ClientClosedRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }

    private static Task<IResult> ListFacultiesAsync(
        IRequestHandler<ListFacultiesQuery, IEnumerable<FacultySummaryResponse>> handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return handler.HandleAsync(new ListFacultiesQuery(), cancellationToken)
            .Match(Results.Ok, e => e.ToProblemDetails());
    }
}

public sealed record ListFacultiesQuery : IRequest;

[UsedImplicitly]
public sealed class ListFacultiesHandler(IPageFetcher pageFetcher, IScrapingService scrapingService) 
    : IRequestHandler<ListFacultiesQuery, IEnumerable<FacultySummaryResponse>>
{
    public async Task<ErrorOr<IEnumerable<FacultySummaryResponse>>> HandleAsync(ListFacultiesQuery request,
        CancellationToken cancellationToken)
    {
        var page = await pageFetcher.FetchAsync(FacultyPages.FacultyListPage, SessionPolicy.None, cancellationToken);
        var faculties = scrapingService.Scrape<IEnumerable<FacultySummary>>(page);
        
        return faculties.Select(f => new FacultySummaryResponse
        {
            Id = f.Id,
            Slug = f.Slug,
            Name = f.Name,
            LongName = f.LongName
        }).ToArray();
    }
}