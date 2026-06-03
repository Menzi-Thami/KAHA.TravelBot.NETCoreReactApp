using KAHA.TravelBot.NETCoreReactApp.Services;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

// ── Strongly-typed config (Patch P4) ──────────────────────────────────────
builder.Services.Configure<TravelBotOptions>(
    builder.Configuration.GetSection("TravelBot"));

// ── Polly resilience policy (Patch P3) ────────────────────────────────────
// Retry up to 3 times with exponential back-off on transient HTTP errors.
// Circuit-breaker opens after 5 consecutive failures for 30 seconds.
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, delay, attempt, _) =>
            Console.WriteLine($"[Polly] Retry {attempt} after {delay.TotalSeconds:F1}s — {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}"));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak:   (_, d) => Console.WriteLine($"[Polly] Circuit open for {d.TotalSeconds}s"),
        onReset:   ()     => Console.WriteLine("[Polly] Circuit reset"),
        onHalfOpen: ()    => Console.WriteLine("[Polly] Circuit half-open"));

builder.Services
    .AddHttpClient<ITravelBotService, TravelBotService>(client =>
    {
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

// ── Register as Singleton (Bonus task 8) ──────────────────────────────────
// IHttpClientFactory manages the underlying handler lifetime; the service
// itself is singleton so the IMemoryCache country list persists for the
// full application lifetime.
builder.Services.AddSingleton<ITravelBotService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client  = factory.CreateClient(nameof(TravelBotService));
    var cache   = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
    var logger  = sp.GetRequiredService<ILogger<TravelBotService>>();
    var options = sp.GetRequiredService<IOptions<TravelBotOptions>>();
    return new TravelBotService(client, cache, logger, options);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
app.MapFallbackToFile("index.html");
app.Run();

public partial class Program { }
