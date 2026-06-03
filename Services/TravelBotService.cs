using KAHA.TravelBot.NETCoreReactApp.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KAHA.TravelBot.NETCoreReactApp.Services;

/// <summary>
/// Singleton service — country list is memory-cached to avoid hammering the
/// external API on every request.
///
/// Patches applied vs original:
///   [P1] Random.Shared — no per-call allocation, better distribution
///   [P2] Sunrise/sunset returned as UTC strings — avoids server-timezone
///        ambiguity (.ToLocalTime() was wrong in a hosted environment)
///   [P3] HTTP resilience (retry + circuit-breaker) configured in Program.cs
///        via Polly — the service itself just calls _httpClient normally
///   [P4] All external URLs read from TravelBotOptions (appsettings.json)
/// </summary>
public class TravelBotService : ITravelBotService
{
    // KAHA office: Cavendish Square, Cape Town
    private const double KahaLatitude  = -33.9759724;
    private const double KahaLongitude =  18.4592032;

    private const string CountriesCacheKey = "all_countries";

    private readonly HttpClient              _httpClient;
    private readonly IMemoryCache            _cache;
    private readonly ILogger<TravelBotService> _logger;
    private readonly TravelBotOptions        _options;

    public TravelBotService(
        HttpClient                    httpClient,
        IMemoryCache                  cache,
        ILogger<TravelBotService>     logger,
        IOptions<TravelBotOptions>    options)
    {
        _httpClient = httpClient;
        _cache      = cache;
        _logger     = logger;
        _options    = options.Value;
    }

    // ── Task 1: GetAllCountries (cached) ──────────────────────────────────────

    public async Task<List<CountryModel>> GetAllCountriesAsync()
    {
        if (_cache.TryGetValue(CountriesCacheKey, out List<CountryModel>? cached) && cached is not null)
            return cached;

        _logger.LogInformation("Cache miss — fetching from {Url}", _options.RestCountriesUrl);

        // [P3] Retry/circuit-breaker is on the HttpClient pipeline (Program.cs)
        var json = await _httpClient.GetStringAsync(_options.RestCountriesUrl); // [P4]
        var countries = JsonConvert.DeserializeObject<List<CountryModel>>(json)
                        ?? throw new InvalidOperationException("RestCountries API returned null.");

        var expiry = TimeSpan.FromHours(_options.CacheHours);
        _cache.Set(CountriesCacheKey, countries, expiry);

        _logger.LogInformation("Cached {Count} countries for {Hours}h", countries.Count, _options.CacheHours);
        return countries;
    }

    // ── Task 2: Top 5 Southern Hemisphere by population ───────────────────────

    public async Task<List<TopFiveCountryModel>> GetTopFiveCountriesAsync()
    {
        var all = await GetAllCountriesAsync();

        return all
            .Where(c => c.IsInSouthernHemisphere)
            .OrderByDescending(c => c.Population)
            .Take(5)
            .Select(c => new TopFiveCountryModel
            {
                Name       = c.CommonName,
                Capital    = c.CapitalCity,
                Population = c.Population,
                Latitude   = c.Latitude,
                Longitude  = c.Longitude
            })
            .ToList();
    }

    // ── Task 4: Country summary ───────────────────────────────────────────────

    public async Task<CountrySummaryModel?> GetCountrySummaryAsync(string countryName)
    {
        var all = await GetAllCountriesAsync();

        // Case-insensitive match on common name
        var country = all.FirstOrDefault(c =>
            c.CommonName.Equals(countryName, StringComparison.OrdinalIgnoreCase));

        if (country is null)
        {
            _logger.LogWarning("Country not found: {CountryName}", countryName);
            return null;
        }

        var (sunrise, sunset) = await GetSunriseSunsetTimesAsync(country.Latitude, country.Longitude);
        var languages = country.Languages?.Values.ToList() ?? [];

        return new CountrySummaryModel
        {
            Name               = country.CommonName,
            Capital            = country.CapitalCity,
            Population         = country.Population,
            Latitude           = country.Latitude,
            Longitude          = country.Longitude,
            Sunrise            = sunrise,
            Sunset             = sunset,
            OfficialLanguages  = languages.Count > 0 ? string.Join(", ", languages) : "N/A",
            TotalLanguages     = languages.Count,
            DriveSide          = country.Car?.Side ?? "unknown",
            DistanceFromKahaKm = CalculateHaversineDistanceKm(
                                    country.Latitude, country.Longitude,
                                    KahaLatitude,     KahaLongitude)
        };
    }

    // ── Task 6 (Bonus): Random Southern Hemisphere country ────────────────────

    public async Task<CountrySummaryModel?> GetRandomSouthernHemisphereCountryAsync()
    {
        var all     = await GetAllCountriesAsync();
        var southern = all.Where(c => c.IsInSouthernHemisphere).ToList();

        if (southern.Count == 0) return null;

        // [P1] Random.Shared — thread-safe, no per-call allocation, better distribution
        var picked = southern[Random.Shared.Next(southern.Count)];
        return await GetCountrySummaryAsync(picked.CommonName);
    }

    // ── Task 3: Sunrise / Sunset ──────────────────────────────────────────────

    /// <summary>
    /// Returns sunrise and sunset as UTC time strings (e.g. "06:23 AM UTC").
    ///
    /// [P2] We deliberately return UTC rather than calling .ToLocalTime().
    /// .ToLocalTime() uses the *server's* timezone — meaningless for a hosted
    /// API and confusing when the country is in a different timezone. Returning
    /// UTC with a clear label is honest; the frontend can localise if needed.
    /// </summary>
    public async Task<(string Sunrise, string Sunset)> GetSunriseSunsetTimesAsync(
        double latitude, double longitude)
    {
        try
        {
            // [P4] URL base from config; formatted=0 gives ISO 8601 UTC strings
            var url      = $"{_options.SunriseSunsetUrl}?lat={latitude}&lng={longitude}&formatted=0";
            var response = await _httpClient.GetStringAsync(url);
            var obj      = JObject.Parse(response);

            if (obj["status"]?.ToString() != "OK")
            {
                _logger.LogWarning("Sunrise-sunset API non-OK for ({Lat},{Lng})", latitude, longitude);
                return ("N/A", "N/A");
            }

            var results = obj["results"]!;

            // [P2] Parse as UTC, format clearly — no server-TZ conversion
            var sunrise = DateTime.Parse(results["sunrise"]!.ToString(),
                              null, System.Globalization.DateTimeStyles.RoundtripKind);
            var sunset  = DateTime.Parse(results["sunset"]!.ToString(),
                              null, System.Globalization.DateTimeStyles.RoundtripKind);

            return (
                sunrise.ToString("hh:mm tt") + " UTC",
                sunset.ToString("hh:mm tt")  + " UTC"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch sunrise/sunset for ({Lat},{Lng})", latitude, longitude);
            return ("N/A", "N/A");
        }
    }

    // ── Task 7 (Bonus): Haversine distance ───────────────────────────────────

    /// <summary>
    /// Great-circle distance between two coordinates using the Haversine formula.
    /// Returns kilometres, rounded to 1 decimal place.
    /// </summary>
    public static double CalculateHaversineDistanceKm(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        const double R = 6371.0; // Earth radius in km

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
              * Math.Sin(dLon / 2)        * Math.Sin(dLon / 2);

        return Math.Round(R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), 1);
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180.0;
}
