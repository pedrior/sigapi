using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Sigapi.Common.Auth;
using Sigapi.Common.Auth.Tokens;
using Sigapi.Common.Endpoints;
using Sigapi.Common.Errors;
using Sigapi.Common.Messaging;
using Sigapi.Common.Messaging.Pipeline;
using Sigapi.Common.RateLimiting;
using Sigapi.Common.Scraping;
using Sigapi.Common.Scraping.Document;
using Sigapi.Common.Scraping.Networking;
using Sigapi.Common.Scraping.Networking.Redirection;
using Sigapi.Common.Scraping.Networking.Resilience;
using Sigapi.Common.Scraping.Networking.Sessions;
using Sigapi.Common.Scraping.Processing;
using Sigapi.Features.Account.Scraping;
using Sigapi.OpenApi;

namespace Sigapi;

public static class Services
{
    private static readonly Assembly ThisAssembly = typeof(Services).Assembly;

    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder app)
    {
        var host = app.Host;
        var services = app.Services;
        var environment = app.Environment;
        var configuration = app.Configuration;

        ConfigureJson(services);
        ConfigureDocs(services);
        ConfigureForwarding(services, environment);
        ConfigureRateLimiter(services);
        ConfigureLogging(host);
        ConfigureErrorHandling(services);
        ConfigureEndpoints(services);
        ConfigureRequests(services);
        ConfigureValidation(services);
        ConfigureCaching(services);
        ConfigureAuth(services, configuration, environment);
        ConfigureScraping(services, configuration);

        return app;
    }

    private static void ConfigureJson(IServiceCollection services) =>
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(
                options.SerializerOptions.PropertyNamingPolicy));
        });

    private static void ConfigureDocs(IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddScalarTransformers();

            options.AddDocumentTransformer<DevelopmentServerDocumentTransformer>();
            options.AddDocumentTransformer<BearerAuthenticationDocumentTransformer>();

            options.AddOperationTransformer<AuthenticationOperationTransformer>();
        });
    }

    private static void ConfigureForwarding(IServiceCollection services, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Configure forwarding for development to ensure Scalar UI uses the correct
            // host and port. See DevelopmentServerDocumentTransformer.
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                options.ForwardedHeaders = ForwardedHeaders.All;
            });
        }
    }

    private static void ConfigureRateLimiter(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter(RateLimiterPolicies.Global, windowOptions =>
            {
                windowOptions.PermitLimit = 10;
                windowOptions.Window = TimeSpan.FromSeconds(10);
                windowOptions.SegmentsPerWindow = 3;
                windowOptions.QueueLimit = 5;
            });

            options.AddTokenBucketLimiter(RateLimiterPolicies.Authentication, tokenOptions =>
            {
                tokenOptions.TokenLimit = 5;
                tokenOptions.TokensPerPeriod = 2;
                tokenOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
                tokenOptions.QueueLimit = 0; // Don't queue login attempts.
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (rejectedContext, cancellationToken) =>
            {
                if (rejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    rejectedContext.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";
                }

                var problemDetailsFactory = rejectedContext.HttpContext.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>();

                var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    rejectedContext.HttpContext,
                    statusCode: StatusCodes.Status429TooManyRequests,
                    detail: "You have exceeded the allowed number of requests.");

                await rejectedContext.HttpContext.Response.WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken: cancellationToken);
            };
        });
    }

    private static void ConfigureLogging(IHostBuilder host)
    {
        host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
    }

    private static void ConfigureErrorHandling(IServiceCollection services)
    {
        services.AddExceptionHandler<ExceptionHandler>();

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
    }

    private static void ConfigureEndpoints(IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssemblies(ThisAssembly)
            .AddClasses(classes => classes.AssignableTo<IEndpoint>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }

    private static void ConfigureRequests(IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssemblies(ThisAssembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationHandler<,>));
    }

    private static void ConfigureValidation(IServiceCollection services) =>
        services.AddValidatorsFromAssembly(ThisAssembly);

    private static void ConfigureCaching(IServiceCollection services)
    {
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            // Use only memory cache for now.
            Flags = HybridCacheEntryFlags.DisableDistributedCache
        });
    }

    private static void ConfigureAuth(IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddTransient<ISecurityTokenProvider, SecurityTokenProvider>();

        var jwtConfig = configuration.GetRequiredSection("Jwt");
        services.AddOptionsWithValidateOnStart<SecurityTokenOptions>()
            .Bind(jwtConfig)
            .ValidateDataAnnotations();

        var securityTokenOptions = jwtConfig.Get<SecurityTokenOptions>()!;
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.MapInboundClaims = false;

                if (environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = !string.IsNullOrEmpty(securityTokenOptions.Issuer),
                    ValidIssuer = securityTokenOptions.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(securityTokenOptions.Audience),
                    ValidAudience = securityTokenOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(securityTokenOptions.Key))
                };

                options.Events = new JwtBearerEvents
                {
                    // Validate the session ID.
                    OnTokenValidated = async context =>
                    {
                        var sid = context.Principal!.Claims.First(c => c.Type is JwtRegisteredClaimNames.Sid);
                        var sessionStore = context.HttpContext.RequestServices.GetRequiredService<ISessionStore>();

                        if (!await sessionStore.IsActiveAsync(sid.Value))
                        {
                            context.Fail("The session has been revoked.");
                        }
                    }
                };
            });

        services.AddAuthorization();
    }

    private static void ConfigureScraping(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<PageFetcherOptions>()
            .Bind(configuration.GetSection("ScrapingClient"))
            .ValidateDataAnnotations();

        services.AddTransient<SessionHandler>();
        services.AddTransient<RedirectHandler>();

        services.AddHttpClient<IPageFetcher, PageFetcher>()
            .ConfigureHttpClient(client =>
            {
                // Handled by resilience policies.
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = false, // Handled manually.
                AllowAutoRedirect = false, // Handled manually.
                AutomaticDecompression = DecompressionMethods.All
            })
            .AddHttpMessageHandler<RedirectHandler>()
            .AddHttpMessageHandler<SessionHandler>()
            .AddScrapingResiliencePolicies();

        services.AddHttpContextAccessor();

        services.AddDataProtection(options => options.ApplicationDiscriminator = "sigapi");

        services.AddScoped<ISessionStore, SessionStore>();

        services.AddKeyedScoped<ISessionProvider, ScopedSessionProvider>(SessionPolicy.Scoped);
        services.AddKeyedScoped<ISessionProvider, UserSessionProvider>(SessionPolicy.User);

        services.AddTransient<ISessionFactory, SessionFactory>();

        services.AddSingleton<IHtmlParser, HtmlParser>();

        services.AddSingleton<IScrapingService, ScrapingService>();
        services.AddSingleton<IElementSelector, ElementSelector>();
        services.AddSingleton<IDataProcessingPipeline, DataProcessingPipeline>();
        services.AddSingleton<ITypeConversionService, TypeConversionService>();

        // Register all IDataProcessor in this assembly as singleton.
        services.Scan(scan => scan.FromAssemblies(ThisAssembly)
            .AddClasses(classes => classes.AssignableTo<IDataProcessor>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        services.AddTransient<IEnrollmentProvider, EnrollmentProvider>();
        services.AddTransient<IEnrollmentSelector, EnrollmentSelector>();
        services.AddTransient<ILoginResponseHandler, LoginResponseHandler>();
        services.AddTransient<ILoginResponseStrategy, CredentialsMismatchStrategy>();
        services.AddTransient<ILoginResponseStrategy, SingleEnrollmentStrategy>();
        services.AddTransient<ILoginResponseStrategy, MultipleEnrollmentStrategy>();
    }
}