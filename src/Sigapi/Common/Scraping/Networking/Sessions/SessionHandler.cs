namespace Sigapi.Common.Scraping.Networking.Sessions;

public sealed class SessionHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    private const string CookieHeader = "Cookie";
    private const string SetCookieHeader = "Set-Cookie";

    public static readonly HttpRequestOptionsKey<ISession> SessionKey = new("Session");

    private readonly ISessionStore? sessionStore = httpContextAccessor.HttpContext?.RequestServices
        .GetRequiredService<ISessionStore>();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is null || !request.Options.TryGetValue(SessionKey, out var session))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        if (session.IsExpired)
        {
            throw new SessionExpiredException("Session has expired.");
        }

        ProcessRequestCookies(request, session);

        var response = await base.SendAsync(request, cancellationToken);

        ProcessResponseCookies(response, session);

        if (response.IsSuccessStatusCode && sessionStore is not null)
        {
            await session.RefreshAsync(sessionStore);
        }

        return response;
    }

    private static void ProcessRequestCookies(HttpRequestMessage request, ISession session)
    {
        if (session.GetCookies(request.RequestUri!) is { } cookies)
        {
            request.Headers.TryAddWithoutValidation(CookieHeader, cookies);
        }
    }

    private static void ProcessResponseCookies(HttpResponseMessage response, ISession session)
    {
        if (response.Headers.TryGetValues(SetCookieHeader, out var cookies))
        {
            session.SetCookies(response.RequestMessage!.RequestUri!, cookies);
        }
    }
}