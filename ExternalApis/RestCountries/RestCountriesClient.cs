using KAHA.TravelBot.NETCoreReactApp.Domain;
using KAHA.TravelBot.NETCoreReactApp.Exceptions;
using KAHA.TravelBot.NETCoreReactApp.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KAHA.TravelBot.NETCoreReactApp.ExternalApis.RestCountries;

/// <summary>
/// Typed <see cref="HttpClient"/> implementation of <see cref="IRestCountriesClient"/>.
/// Owns the wire contract (the private DTOs below) and maps it to <see cref="Country"/>
/// so the external JSON shape never escapes this class.
/// </summary>
public sealed class RestCountriesClient : IRestCountriesClient
{
    private readonly HttpClient _httpClient;
    private readonly TravelBotOptions _options;
    private readonly ILogger<RestCountriesClient> _logger;

    public RestCountriesClient(
        HttpClient httpClient,
        IOptions<TravelBotOptions> options,
        ILogger<RestCountriesClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching countries from {Url}", _options.RestCountriesUrl);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(_options.RestCountriesUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalApiException("RestCountries API request failed.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("RestCountries API failed: {Status} - {Error}", response.StatusCode, error);
            throw new ExternalApiException($"RestCountries API returned {(int)response.StatusCode}.");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        List<RestCountryContract>? raw;
        try
        {
            raw = JsonConvert.DeserializeObject<List<RestCountryContract>>(json);
        }
        catch (JsonException ex)
        {
            throw new ExternalApiException("RestCountries API returned an unreadable payload.", ex);
        }

        if (raw is null)
            throw new ExternalApiException("RestCountries API returned an unreadable payload.");

        var countries = raw.Select(Map).ToList();
        _logger.LogInformation("Fetched {Count} countries", countries.Count);
        return countries;
    }

    private static Country Map(RestCountryContract c) => new()
    {
        Name = c.Name?.Common ?? "Unknown",
        Capital = c.Capital?.FirstOrDefault() ?? "N/A",
        Population = c.Population,
        Latitude = c.LatLng is { Count: >= 1 } ? c.LatLng[0] : 0,
        Longitude = c.LatLng is { Count: >= 2 } ? c.LatLng[1] : 0,
        Languages = c.Languages?.Values.ToList() ?? [],
        DriveSide = c.Car?.Side ?? "unknown"
    };

    // ── Private wire contracts (RestCountries v3.1) ───────────────────────────
    private sealed class RestCountryContract
    {
        [JsonProperty("name")]
        public NameContract? Name { get; set; }

        [JsonProperty("capital")]
        public List<string>? Capital { get; set; }

        [JsonProperty("population")]
        public long Population { get; set; }

        /// <summary>latlng[0] = latitude, latlng[1] = longitude.</summary>
        [JsonProperty("latlng")]
        public List<double>? LatLng { get; set; }

        [JsonProperty("languages")]
        public Dictionary<string, string>? Languages { get; set; }

        [JsonProperty("car")]
        public CarContract? Car { get; set; }
    }

    private sealed class NameContract
    {
        [JsonProperty("common")]
        public string Common { get; set; } = string.Empty;

        [JsonProperty("official")]
        public string Official { get; set; } = string.Empty;
    }

    private sealed class CarContract
    {
        [JsonProperty("side")]
        public string Side { get; set; } = "unknown";
    }
}
