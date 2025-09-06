using Scalar.AspNetCore;
using Serilog;
using Sigapi;
using Sigapi.Common.Endpoints;

var builder = WebApplication.CreateBuilder(args)
    .ConfigureServices();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseStatusCodePages();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("SigAPI Documentation")
            .WithTagSorter(TagSorter.Alpha)
            .WithDefaultOpenAllTags()
            .WithDefaultHttpClient(ScalarTarget.Node, ScalarClient.Axios)
            .WithPersistentAuthentication()
            .AddPreferredSecuritySchemes("Bearer");
    });

    app.MapGet("/", () => Results.Redirect("/docs", permanent: true))
        .ExcludeFromApiReference();
}

app.MapEndpoints(app.Services);

app.Run();