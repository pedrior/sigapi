using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;

namespace Sigapi.Common.Scraping.Networking.Resilience;

public static class HttpClientResilienceExtensions
{
    public static IHttpStandardResiliencePipelineBuilder AddScrapingResiliencePolicies(this IHttpClientBuilder builder)
    {
        return builder.AddStandardResilienceHandler(options =>
        {
            // The total time allowed for a request, including all retries and delays.
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);

            // The timeout for each single attempt.
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);

            ConfigureScrapingClientRetryPolicy(options.Retry);
            ConfigureScrapingClientCircuitBreakerPolicy(options.CircuitBreaker);
        });
    }
    
    private static void ConfigureScrapingClientRetryPolicy(HttpRetryStrategyOptions options)
    {
        options.MaxRetryAttempts = 5;
        options.Delay = TimeSpan.FromSeconds(2);
        options.BackoffType = DelayBackoffType.Exponential;
        options.UseJitter = true;

        // Handle transient errors and specific HTTP status codes.
        options.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutRejectedException>()
            .HandleResult(response => response.StatusCode is >= HttpStatusCode.InternalServerError
                or HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests);
    }

    private static void ConfigureScrapingClientCircuitBreakerPolicy(HttpCircuitBreakerStrategyOptions options)
    {
        options.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutRejectedException>()
            .HandleResult(message => message.StatusCode is >= HttpStatusCode.InternalServerError
                or HttpStatusCode.RequestTimeout);

        options.FailureRatio = 0.5;
        options.MinimumThroughput = 20;

        // The duration over which failure rates are tracked.
        options.SamplingDuration = TimeSpan.FromSeconds(30);

        // The duration of the circuit will stay open before transitioning to half-open.
        options.BreakDuration = TimeSpan.FromSeconds(60);
    }
}