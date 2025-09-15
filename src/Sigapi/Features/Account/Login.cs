using Sigapi.Common.Endpoints;
using Sigapi.Common.Errors;
using Sigapi.Common.Messaging;
using Sigapi.Common.RateLimiting;
using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Networking.Sessions;
using Sigapi.Common.Security.Tokens;
using Sigapi.Contracts.Account;
using Sigapi.Features.Account.Models;
using Sigapi.Features.Account.Scraping;
using ISession = Sigapi.Common.Scraping.Networking.Sessions.ISession;

namespace Sigapi.Features.Account;

[UsedImplicitly]
public sealed class LoginEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder route)
    {
        route.MapPost("account/login", LoginAsync)
            .RequireRateLimiting(RateLimiterPolicies.Authentication)
            .WithSummary("Login")
            .WithDescription("Authenticates a student using their credentials and returns an access token " +
                             "for subsequent requests.")
            .WithTags("Account")
            .Accepts<LoginRequest>("application/json")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }

    private static Task<IResult> LoginAsync(LoginRequest request,
        IRequestHandler<LoginCommand, LoginResponse> handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Username, request.Password, request.Enrollment);
        return handler.HandleAsync(command, cancellationToken)
            .Match(Results.Ok, errors => errors.ToProblemDetails());
    }
}

[UsedImplicitly]
public sealed record LoginCommand(string Username, string Password, string? Enrollment = null) : IRequest;

[UsedImplicitly]
public sealed class LoginCommandHandler(
    IPageFetcher pageFetcher,
    IScrapingService scrapingService,
    ILoginResponseHandler loginResponseHandler,
    ISecurityTokenProvider securityTokenProvider,
    ISessionStore sessionStore) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<ErrorOr<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var loginPage = await pageFetcher.FetchAsync(AccountPages.Login, cancellationToken: cancellationToken);
        var loginForm = scrapingService.Scrape<LoginForm>(loginPage);

        var loginResponsePage = await pageFetcher.FetchWithFormSubmissionAsync(
            loginForm.Action,
            loginForm.BuildSubmissionData(command.Username, command.Password),
            cancellationToken: cancellationToken);

        var result = await loginResponseHandler.HandleAsync(
            loginResponsePage,
            command.Username,
            command.Enrollment,
            cancellationToken);

        return await result.ThenDoAsync(_ => PersistUserSessionAsync(loginResponsePage.Session!, cancellationToken))
            .Then(user => CreateAccessToken(loginResponsePage.Session!.Id, user));
    }

    private Task PersistUserSessionAsync(ISession session, CancellationToken cancellationToken) =>
        sessionStore.SaveAsync(session, cancellationToken);

    private LoginResponse CreateAccessToken(string sessionId, User user)
    {
        var claims = new Dictionary<string, object>
        {
            [CustomClaimTypes.Username] = user.Username,
            [CustomClaimTypes.Enrollment] = user.Enrollment.Number,
            [CustomClaimTypes.Enrollments] = CreateEnrollmentsClaimValue(user),
            [JwtRegisteredClaimNames.Sid] = sessionId
        };

        var (token, expiresAt) = securityTokenProvider.CreateToken(claims);
        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        };

        static string[] CreateEnrollmentsClaimValue(User user) =>
            user.Enrollments.OrderByDescending(e => e.Number)
                .Select(e => e.Number)
                .ToArray();
    }
}

[UsedImplicitly]
public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Must not be null or empty.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Must not be null or empty.");

        RuleFor(x => x.Enrollment)
            .Must(v => string.IsNullOrEmpty(v) || v.All(char.IsDigit))
            .WithMessage("Must be a valid enrollment identifier consisting of digits only.");
    }
}