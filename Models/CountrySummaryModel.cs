namespace KAHA.TravelBot.NETCoreReactApp.Models;

/// <summary>
/// Client-facing DTO for full country detail.
/// Deliberately does NOT inherit CountryModel — keeps internal API shape
/// decoupled from the public contract.
/// </summary>
public class CountrySummaryModel
{
    public string Name               { get; set; } = string.Empty;
    public string Capital            { get; set; } = string.Empty;
    public long   Population         { get; set; }
    public double Latitude           { get; set; }
    public double Longitude          { get; set; }
    public string Sunrise            { get; set; } = string.Empty;
    public string Sunset             { get; set; } = string.Empty;
    public string OfficialLanguages  { get; set; } = string.Empty;
    public int    TotalLanguages     { get; set; }
    public string DriveSide          { get; set; } = string.Empty;
    public double DistanceFromKahaKm { get; set; }
}
