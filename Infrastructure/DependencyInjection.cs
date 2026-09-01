using System.Net.Http.Headers;
using KAHA.TravelBot.NETCoreReactApp.ExternalApis.RestCountries;
using KAHA.TravelBot.NETCoreReactApp.ExternalApis.SunriseSunset;
using KAHA.TravelBot.NETCoreReactApp.Services;
using Polly;
using Polly.Extensions.Http;

namespace KAHA.TravelBot.NETCoreReactApp.Infrastructure;

/// <summary>
/// Composition root helpers. Keeps <c>Program.cs</c> declarative and puts all
/// wiring for the travel-bot feature (options, cache, resilient typed clients,
/// services, exception handling) in one place.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTravelBot(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TravelBotOptions>(configuration.GetSection("TravelBot"));
        services.AddMemoryCache();

        var options = configuration.GetSection("TravelBot").Get<TravelBotOptions>() ?? new TravelBotOptions();
        var timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);

        services.AddHttpClient<IRestCountriesClient, RestCountriesClient>(client =>
            {
                client.Timeout = timeout;
                client.DefaultRequestHeaders.UserAgent.ParseAdd("TravelBot-App");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddResilience();

        services.AddHttpClient<ISunriseSunsetClient, SunriseSunsetClient>(client =>
            {
                client.Timeout = timeout;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddResilience();

        services.AddScoped<ITravelBotService, TravelBotService>();

        return services;
    }

    /// <summary>
    /// Registers the exception-to-HTTP mapping used across the API. Pairs the
    /// <see cref="GlobalExceptionHandler"/> with framework ProblemDetails.
    /// </summary>
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    // ── Polly resilience: retry with exponential back-off + circuit breaker ──
    private static IHttpClientBuilder AddResilience(this IHttpClientBuilder builder) =>
        builder
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))))
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30)));
}
