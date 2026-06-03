using Newtonsoft.Json;

namespace KAHA.TravelBot.NETCoreReactApp.Models;

/// <summary>
/// Internal domain model — maps the RestCountries v3.1 API response.
/// Only the fields we actually use are mapped; all others are silently ignored.
/// </summary>
public class CountryModel
{
    [JsonProperty("name")]
    public CountryName Name { get; set; } = new();

    [JsonProperty("capital")]
    public List<string>? Capital { get; set; }

    [JsonProperty("population")]
    public long Population { get; set; }

    /// <summary>latlng[0] = latitude, latlng[1] = longitude.</summary>
    [JsonProperty("latlng")]
    public List<double>? LatLng { get; set; }

    /// <summary>ISO 639-3 code → language name, e.g. { "eng": "English" }.</summary>
    [JsonProperty("languages")]
    public Dictionary<string, string>? Languages { get; set; }

    /// <summary>{ "side": "left" | "right" }</summary>
    [JsonProperty("car")]
    public CarInfo? Car { get; set; }

    // ── Computed helpers (not serialised to clients) ──────────────────────────

    [JsonIgnore]
    public string CommonName => Name?.Common ?? "Unknown";

    [JsonIgnore]
    public string CapitalCity => Capital?.FirstOrDefault() ?? "N/A";

    /// <summary>
    /// Safe latitude access — guards against missing or empty latlng array.
    /// </summary>
    [JsonIgnore]
    public double Latitude => LatLng is { Count: >= 1 } ? LatLng[0] : 0;

    /// <summary>
    /// Safe longitude access — guards against missing or short latlng array.
    /// </summary>
    [JsonIgnore]
    public double Longitude => LatLng is { Count: >= 2 } ? LatLng[1] : 0;

    [JsonIgnore]
    public bool IsInSouthernHemisphere => Latitude < 0;
}

public class CountryName
{
    [JsonProperty("common")]
    public string Common { get; set; } = string.Empty;

    [JsonProperty("official")]
    public string Official { get; set; } = string.Empty;
}

public class CarInfo
{
    [JsonProperty("side")]
    public string Side { get; set; } = "unknown";
}
