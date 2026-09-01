using KAHA.TravelBot.NETCoreReactApp.Models;

namespace KAHA.TravelBot.NETCoreReactApp.Services;

/// <summary>
/// Application service that turns country/geo data into the travel-facing
/// view models the API returns. Depends only on the external-API abstractions,
/// never on <see cref="HttpClient"/> directly.
/// </summary>
public interface ITravelBotService
{
    /// <summary>Top 5 most populous countries in the Southern Hemisphere.</summary>
    Task<IReadOnlyList<TopFiveCountryModel>> GetTopFiveCountriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full summary for a country by its common name (case-insensitive).
    /// Throws <see cref="Exceptions.NotFoundException"/> if no match exists.
    /// </summary>
    Task<CountrySummaryModel> GetCountrySummaryAsync(
        string countryName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Full summary for a randomly chosen Southern Hemisphere country.
    /// Throws <see cref="Exceptions.NotFoundException"/> if none are available.
    /// </summary>
    Task<CountrySummaryModel> GetRandomSouthernHemisphereCountryAsync(
        CancellationToken cancellationToken = default);
}
