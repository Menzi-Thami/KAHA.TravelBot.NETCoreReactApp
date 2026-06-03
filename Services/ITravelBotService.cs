using KAHA.TravelBot.NETCoreReactApp.Models;

namespace KAHA.TravelBot.NETCoreReactApp.Services;

public interface ITravelBotService
{
    /// <summary>All countries from the RestCountries API (memory-cached).</summary>
    Task<List<CountryModel>> GetAllCountriesAsync();

    /// <summary>Top 5 most populous countries in the Southern Hemisphere.</summary>
    Task<List<TopFiveCountryModel>> GetTopFiveCountriesAsync();

    /// <summary>Full summary for a country by its common name (case-insensitive).</summary>
    Task<CountrySummaryModel?> GetCountrySummaryAsync(string countryName);

    /// <summary>Full summary for a randomly chosen Southern Hemisphere country.</summary>
    Task<CountrySummaryModel?> GetRandomSouthernHemisphereCountryAsync();
}
