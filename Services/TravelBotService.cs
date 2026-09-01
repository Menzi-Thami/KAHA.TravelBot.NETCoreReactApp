using KAHA.TravelBot.NETCoreReactApp.Domain;
using KAHA.TravelBot.NETCoreReactApp.Exceptions;
using KAHA.TravelBot.NETCoreReactApp.ExternalApis.RestCountries;
using KAHA.TravelBot.NETCoreReactApp.ExternalApis.SunriseSunset;
using KAHA.TravelBot.NETCoreReactApp.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace KAHA.TravelBot.NETCoreReactApp.Services;

/// <summary>
/// Business logic for the travel bot. Composes the RestCountries and
/// sunrise/sunset clients, applies caching, and shapes the public DTOs.
/// Holds no I/O of its own — all outbound calls go through injected abstractions.
/// </summary>
public sealed class TravelBotService : ITravelBotService
{
    // KAHA office: Cavendish Square, Cape Town.
    private const double KahaLatitude = -33.9759724;
    private const double KahaLongitude = 18.4592032;

    private const string CountriesCacheKey = "all_countries";

    private readonly IRestCountriesClient _countriesClient;
    private readonly ISunriseSunsetClient _sunriseSunsetClient;
    private readonly IMemoryCache _cache;
    private readonly TravelBotOptions _options;
    private readonly ILogger<TravelBotService> _logger;

    public TravelBotService(
        IRestCountriesClient countriesClient,
        ISunriseSunsetClient sunriseSunsetClient,
        IMemoryCache cache,
        IOptions<TravelBotOptions> options,
        ILogger<TravelBotService> logger)
    {
        _countriesClient = countriesClient;
        _sunriseSunsetClient = sunriseSunsetClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TopFiveCountryModel>> GetTopFiveCountriesAsync(
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllCountriesAsync(cancellationToken);

        return all
            .Where(c => c.IsInSouthernHemisphere)
            .OrderByDescending(c => c.Population)
            .Take(5)
            .Select(c => new TopFiveCountryModel
            {
                Name = c.Name,
                Capital = c.Capital,
                Population = c.Population,
                Latitude = c.Latitude,
                Longitude = c.Longitude
            })
            .ToList();
    }

    public async Task<CountrySummaryModel> GetCountrySummaryAsync(
        string countryName, CancellationToken cancellationToken = default)
    {
        var all = await GetAllCountriesAsync(cancellationToken);

        var country = all.FirstOrDefault(c =>
            c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException($"Country '{countryName}' not found.");

        return await BuildSummaryAsync(country, cancellationToken);
    }

    public async Task<CountrySummaryModel> GetRandomSouthernHemisphereCountryAsync(
        CancellationToken cancellationToken = default)
    {
        var all = await GetAllCountriesAsync(cancellationToken);
        var southern = all.Where(c => c.IsInSouthernHemisphere).ToList();

        if (southern.Count == 0)
            throw new NotFoundException("No Southern Hemisphere countries found.");

        var picked = southern[Random.Shared.Next(southern.Count)];
        return await BuildSummaryAsync(picked, cancellationToken);
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private async Task<IReadOnlyList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CountriesCacheKey, out IReadOnlyList<Country>? cached) && cached is not null)
            return cached;

        var countries = await _countriesClient.GetAllCountriesAsync(cancellationToken);

        _cache.Set(CountriesCacheKey, countries, TimeSpan.FromHours(_options.CacheHours));
        _logger.LogInformation("Cached {Count} countries for {Hours}h", countries.Count, _options.CacheHours);

        return countries;
    }

    private async Task<CountrySummaryModel> BuildSummaryAsync(Country country, CancellationToken cancellationToken)
    {
        var times = await _sunriseSunsetClient.GetTimesAsync(
            country.Latitude, country.Longitude, cancellationToken);

        return new CountrySummaryModel
        {
            Name = country.Name,
            Capital = country.Capital,
            Population = country.Population,
            Latitude = country.Latitude,
            Longitude = country.Longitude,
            Sunrise = FormatTime(times?.Sunrise),
            Sunset = FormatTime(times?.Sunset),
            OfficialLanguages = country.Languages.Count > 0 ? string.Join(", ", country.Languages) : "N/A",
            TotalLanguages = country.Languages.Count,
            DriveSide = country.DriveSide,
            DistanceFromKahaKm = CalculateHaversineDistanceKm(
                country.Latitude, country.Longitude, KahaLatitude, KahaLongitude)
        };
    }

    private static string FormatTime(DateTime? utc) =>
        utc is null ? "N/A" : utc.Value.ToString("hh:mm tt") + " UTC";

    // Pure geometry — no ambient state, safe as a static helper.
    private static double CalculateHaversineDistanceKm(
        double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return Math.Round(earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), 1);
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180.0;
}
