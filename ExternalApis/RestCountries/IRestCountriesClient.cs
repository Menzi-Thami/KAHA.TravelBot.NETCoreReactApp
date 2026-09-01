using KAHA.TravelBot.NETCoreReactApp.Domain;

namespace KAHA.TravelBot.NETCoreReactApp.ExternalApis.RestCountries;

/// <summary>
/// Abstraction over the RestCountries API. Consumers depend on this, never on
/// <see cref="HttpClient"/> directly (Dependency Inversion).
/// </summary>
public interface IRestCountriesClient
{
    /// <summary>Fetches every country and maps it into the clean domain model.</summary>
    Task<IReadOnlyList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default);
}
