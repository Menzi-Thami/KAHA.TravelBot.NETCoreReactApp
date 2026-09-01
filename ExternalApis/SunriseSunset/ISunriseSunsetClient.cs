namespace KAHA.TravelBot.NETCoreReactApp.ExternalApis.SunriseSunset;

/// <summary>Parsed sunrise/sunset instants for a location (UTC).</summary>
public readonly record struct SunriseSunset(DateTime Sunrise, DateTime Sunset);

/// <summary>
/// Abstraction over the sunrise-sunset.org API. Returns <c>null</c> when the
/// times are unavailable so callers can degrade gracefully — this is an
/// expected, non-exceptional outcome, not a swallowed error.
/// </summary>
public interface ISunriseSunsetClient
{
    Task<SunriseSunset?> GetTimesAsync(
        double latitude, double longitude, CancellationToken cancellationToken = default);
}
