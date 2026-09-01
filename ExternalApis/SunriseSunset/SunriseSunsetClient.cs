using System.Globalization;
using KAHA.TravelBot.NETCoreReactApp.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace KAHA.TravelBot.NETCoreReactApp.ExternalApis.SunriseSunset;

/// <summary>
/// Typed <see cref="HttpClient"/> implementation of <see cref="ISunriseSunsetClient"/>.
/// The sunrise/sunset lookup is a "nice to have" enrichment for a country
/// summary, so transport/parse failures degrade to <c>null</c> (logged) rather
/// than failing the whole request.
/// </summary>
public sealed class SunriseSunsetClient : ISunriseSunsetClient
{
    private readonly HttpClient _httpClient;
    private readonly TravelBotOptions _options;
    private readonly ILogger<SunriseSunsetClient> _logger;

    public SunriseSunsetClient(
        HttpClient httpClient,
        IOptions<TravelBotOptions> options,
        ILogger<SunriseSunsetClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SunriseSunset?> GetTimesAsync(
        double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_options.SunriseSunsetUrl}?lat={latitude}&lng={longitude}&formatted=0";

            var payload = await _httpClient.GetStringAsync(url, cancellationToken);
            var obj = JObject.Parse(payload);

            if (obj["status"]?.ToString() != "OK")
            {
                _logger.LogWarning("Sunrise API non-OK for ({Lat},{Lng})", latitude, longitude);
                return null;
            }

            var results = obj["results"]!;

            var sunrise = DateTime.Parse(
                results["sunrise"]!.ToString(), null, DateTimeStyles.RoundtripKind);
            var sunset = DateTime.Parse(
                results["sunset"]!.ToString(), null, DateTimeStyles.RoundtripKind);

            return new SunriseSunset(sunrise, sunset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sunrise fetch failed for ({Lat},{Lng})", latitude, longitude);
            return null;
        }
    }
}
