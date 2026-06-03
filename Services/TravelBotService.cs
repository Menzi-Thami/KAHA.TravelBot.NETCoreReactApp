using KAHA.TravelBot.NETCoreReactApp.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KAHA.TravelBot.NETCoreReactApp.Services;

public class TravelBotService : ITravelBotService
{
    // KAHA office: Cavendish Square, Cape Town
    private const double KahaLatitude = -33.9759724;
    private const double KahaLongitude = 18.4592032;

    private const string CountriesCacheKey = "all_countries";

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TravelBotService> _logger;
    private readonly TravelBotOptions _options;

    public TravelBotService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<TravelBotService> logger,
        IOptions<TravelBotOptions> options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _options = options.Value;

        // ✅ FIX: Ensure headers are set (prevents 400 from RestCountries)
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TravelBot-App");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // ── Task 1: GetAllCountries (cached) ──────────────────────────────────────
    public async Task<List<CountryModel>> GetAllCountriesAsync()
{
    try
    {
        if (_cache.TryGetValue(CountriesCacheKey, out List<CountryModel>? cached) && cached is not null)
            return cached;

        _logger.LogInformation("Cache miss — fetching from {Url}", _options.RestCountriesUrl);

        var response = await _httpClient.GetAsync(_options.RestCountriesUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("RestCountries API failed: {Status} - {Error}", response.StatusCode, error);
            throw new Exception($"API failed: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

// ✅ DEBUG: Log first part of JSON
_logger.LogInformation("RAW JSON SAMPLE: {Json}",
    json.Substring(0, Math.Min(json.Length, 1000)));

        var countries = JsonConvert.DeserializeObject<List<CountryModel>>(json)
                        ?? throw new InvalidOperationException("RestCountries API returned null.");

        var expiry = TimeSpan.FromHours(_options.CacheHours);
        _cache.Set(CountriesCacheKey, countries, expiry);

        _logger.LogInformation("Cached {Count} countries for {Hours}h", countries.Count, _options.CacheHours);

        return countries;
}
catch (Exception ex)
{
    _logger.LogError(ex, "GetAllCountriesAsync FAILED");
    throw;
}
    }

    // ── Task 2: Top 5 Southern Hemisphere ─────────────────────────────────────
    public async Task<List<TopFiveCountryModel>> GetTopFiveCountriesAsync()
    {
        var all = await GetAllCountriesAsync();

        return all
            .Where(c => c.IsInSouthernHemisphere)
            .OrderByDescending(c => c.Population)
            .Take(5)
            .Select(c => new TopFiveCountryModel
            {
                Name = c.CommonName,
                Capital = c.CapitalCity,
                Population = c.Population,
                Latitude = c.Latitude,
                Longitude = c.Longitude
            })
            .ToList();
    }

    // ── Task 4: Country Summary ───────────────────────────────────────────────
    public async Task<CountrySummaryModel?> GetCountrySummaryAsync(string countryName)
    {
        var all = await GetAllCountriesAsync();

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
            Name = country.CommonName,
            Capital = country.CapitalCity,
            Population = country.Population,
            Latitude = country.Latitude,
            Longitude = country.Longitude,
            Sunrise = sunrise,
            Sunset = sunset,
            OfficialLanguages = languages.Count > 0 ? string.Join(", ", languages) : "N/A",
            TotalLanguages = languages.Count,
            DriveSide = country.Car?.Side ?? "unknown",
            DistanceFromKahaKm = CalculateHaversineDistanceKm(
                country.Latitude, country.Longitude,
                KahaLatitude, KahaLongitude)
        };
    }

    // ── Task 6: Random Southern Hemisphere ────────────────────────────────────
    public async Task<CountrySummaryModel?> GetRandomSouthernHemisphereCountryAsync()
    {
        var all = await GetAllCountriesAsync();
        var southern = all.Where(c => c.IsInSouthernHemisphere).ToList();

        if (southern.Count == 0) return null;

        var picked = southern[Random.Shared.Next(southern.Count)];
        return await GetCountrySummaryAsync(picked.CommonName);
    }

    // ── Task 3: Sunrise / Sunset ──────────────────────────────────────────────
    public async Task<(string Sunrise, string Sunset)> GetSunriseSunsetTimesAsync(
        double latitude, double longitude)
    {
        try
        {
            // ✅ FIX: Correct query string (& not &amp;)
            var url = $"{_options.SunriseSunsetUrl}?lat={latitude}&lng={longitude}&formatted=0";

            var response = await _httpClient.GetStringAsync(url);
            var obj = JObject.Parse(response);

            if (obj["status"]?.ToString() != "OK")
            {
                _logger.LogWarning("Sunrise API non-OK for ({Lat},{Lng})", latitude, longitude);
                return ("N/A", "N/A");
            }

            var results = obj["results"]!;

            var sunrise = DateTime.Parse(results["sunrise"]!.ToString(),
                null, System.Globalization.DateTimeStyles.RoundtripKind);

            var sunset = DateTime.Parse(results["sunset"]!.ToString(),
                null, System.Globalization.DateTimeStyles.RoundtripKind);

            return (
                sunrise.ToString("hh:mm tt") + " UTC",
                sunset.ToString("hh:mm tt") + " UTC"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sunrise fetch failed for ({Lat},{Lng})", latitude, longitude);
            return ("N/A", "N/A");
        }
    }

    // ── Task 7: Haversine Distance ────────────────────────────────────────────
    public static double CalculateHaversineDistanceKm(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        const double R = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return Math.Round(R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), 1);
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180.0;
}
