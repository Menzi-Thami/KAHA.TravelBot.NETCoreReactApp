namespace KAHA.TravelBot.NETCoreReactApp.Models;

/// <summary>
/// Lightweight DTO for the Top 5 table — only what the UI needs.
/// </summary>
public class TopFiveCountryModel
{
    public string Name       { get; set; } = string.Empty;
    public string Capital    { get; set; } = string.Empty;
    public long   Population { get; set; }
    public double Latitude   { get; set; }
    public double Longitude  { get; set; }
}
