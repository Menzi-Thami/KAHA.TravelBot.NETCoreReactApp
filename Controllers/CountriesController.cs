using KAHA.TravelBot.NETCoreReactApp.Models;
using KAHA.TravelBot.NETCoreReactApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KAHA.TravelBot.NETCoreReactApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {

        private readonly ILogger<CountriesController> _logger;
        private readonly TravelBotService _travelBotService;

        public CountriesController(ILogger<CountriesController> logger, TravelBotService travelBotService)
        {
            _logger = logger;
            _travelBotService = travelBotService;
        }

        // GET: api/<CountriesController>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<CountryModel>>> GetAllCountries()
        {
            try
            {
                var countries = await _travelBotService.GetAllCountries();
                return Ok(countries);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while fetching all countries");
                return StatusCode(500, "An error occurred while fetching all countries. Please try again later.");
            }
        }

        // GET: api/<CountriesController>/top5
        [HttpGet("top5")]
        public async Task<ActionResult<IEnumerable<CountryModel>>> GetTopFive()
        {
            var countries = await _travelBotService.GetTopFiveCountries();
            return Ok(countries);
        }

        // GET: api/<CountriesController>/summary
        [HttpGet("summary")]
        public async Task<ActionResult<List<CountrySummaryModel>>> GetSummary([FromQuery] List<string> countryNames)
        {
            var (summaries, errorMessage) = await _travelBotService.GetCountrySummaries(countryNames);

            if (summaries == null)
            {
                return NotFound(errorMessage); // Return the error message if any country summary is not found
            }

            return Ok(summaries); // Return the list of country summaries
        }

        // POST: api/<CountriesController>
        [HttpPost("random")]
        public async Task<ActionResult<CountryModel>> GetRandomCountry()
        {
            try
            {
                var randomCountry = await _travelBotService.GetRandomCountry();

                if (randomCountry != null)
                {
                    return Ok(randomCountry);
                }
                else
                {
                    return NotFound("No random country found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching a random country");
                return StatusCode(500, "An error occurred while fetching a random country. Please try again later.");
            }
        }

        [HttpGet("random-southern-hemisphere")]
        public ActionResult<CountryModel> GetRandomCountryInSouthernHemisphere()
        {
            try
            {
                var allCountries = _travelBotService.GetAllCountries().Result; // Blocking call, consider making this asynchronous

                // Get a random country in the Southern Hemisphere
                var randomCountry = _travelBotService.RandomCountryInSouthernHemisphere(allCountries);

                if (randomCountry != null)
                {
                    return Ok(randomCountry);
                }
                else
                {
                    return NotFound("No random country found in the Southern Hemisphere");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching a random country in the Southern Hemisphere");
                return StatusCode(500, "An error occurred while fetching a random country in the Southern Hemisphere. Please try again later.");
            }
        }


    }
}
