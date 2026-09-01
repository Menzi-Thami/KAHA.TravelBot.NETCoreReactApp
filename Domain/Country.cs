namespace KAHA.TravelBot.NETCoreReactApp.Domain;

/// <summary>
/// Clean internal representation of a country. Deliberately free of any
/// external-API/serialisation concerns — the RestCountries JSON shape is
/// mapped into this type inside <c>RestCountriesClient</c> so the wire format
/// never leaks into the service or the controllers.
/// </summary>
public sealed class Country
{
    public required string Name { get; init; }
    public required string Capital { get; init; }
    public long Population { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    /// <summary>Official language names, e.g. ["English", "Afrikaans"].</summary>
    public IReadOnlyList<string> Languages { get; init; } = [];

    /// <summary>Side of the road people drive on ("left" / "right" / "unknown").</summary>
    public string DriveSide { get; init; } = "unknown";

    public bool IsInSouthernHemisphere => Latitude < 0;
}
