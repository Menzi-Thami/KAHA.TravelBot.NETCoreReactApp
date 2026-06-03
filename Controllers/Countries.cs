using KAHA.TravelBot.NETCoreReactApp.Models;
using KAHA.TravelBot.NETCoreReactApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace KAHA.TravelBot.NETCoreReactApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ITravelBotService           _service;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ITravelBotService service, ILogger<CountriesController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    /// <summary>Top 5 most populous Southern Hemisphere countries.</summary>
    [HttpGet("top5")]
    [ProducesResponseType(typeof(IEnumerable<TopFiveCountryModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopFive()
    {
        try   { return Ok(await _service.GetTopFiveCountriesAsync()); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top 5");
            return StatusCode(500, "An error occurred fetching top 5 countries.");
        }
    }

    /// <summary>Full country summary by name (case-insensitive).</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CountrySummaryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary([FromQuery] string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
            return BadRequest("countryName is required.");

        try
        {
            var summary = await _service.GetCountrySummaryAsync(countryName);
            return summary is null
                ? NotFound($"Country '{countryName}' not found.")
                : Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching summary for {Name}", countryName);
            return StatusCode(500, $"An error occurred fetching summary for '{countryName}'.");
        }
    }

    /// <summary>Random Southern Hemisphere country summary.</summary>
    [HttpGet("surprise")]
    [ProducesResponseType(typeof(CountrySummaryModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSurprise()
    {
        try
        {
            var summary = await _service.GetRandomSouthernHemisphereCountryAsync();
            return summary is null
                ? NotFound("No Southern Hemisphere countries found.")
                : Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching surprise country");
            return StatusCode(500, "An error occurred fetching a random country.");
        }
    }
}
