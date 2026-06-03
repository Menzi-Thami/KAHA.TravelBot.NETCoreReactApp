namespace KAHA.TravelBot.NETCoreReactApp.Services;

/// <summary>
/// Strongly-typed config bound from appsettings.json "TravelBot" section.
/// Removes all hardcoded URLs from the service.
/// </summary>
public class TravelBotOptions
{
    public string RestCountriesUrl    { get; set; } = "https://restcountries.com/v3.1/all";
    public string SunriseSunsetUrl    { get; set; } = "https://api.sunrise-sunset.org/json";
    public int    CacheHours          { get; set; } = 6;
    public int    HttpTimeoutSeconds  { get; set; } = 30;
}
