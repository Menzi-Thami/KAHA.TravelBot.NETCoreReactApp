using KAHA.TravelBot.NETCoreReactApp.Models;
using KAHA.TravelBot.NETCoreReactApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace KAHA.TravelBot.NETCoreReactApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ITravelBotService _service;

    public CountriesController(ITravelBotService service) => _service = service;

    /// <summary>Top 5 most populous Southern Hemisphere countries.</summary>
    [HttpGet("top5")]
    [ProducesResponseType(typeof(IEnumerable<TopFiveCountryModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopFive(CancellationToken cancellationToken)
        => Ok(await _service.GetTopFiveCountriesAsync(cancellationToken));

    /// <summary>Full country summary by name (case-insensitive).</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CountrySummaryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string countryName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(countryName))
            return BadRequest("countryName is required.");

        return Ok(await _service.GetCountrySummaryAsync(countryName, cancellationToken));
    }

    /// <summary>Random Southern Hemisphere country summary.</summary>
    [HttpGet("surprise")]
    [ProducesResponseType(typeof(CountrySummaryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSurprise(CancellationToken cancellationToken)
        => Ok(await _service.GetRandomSouthernHemisphereCountryAsync(cancellationToken));
}
