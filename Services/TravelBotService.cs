using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KAHA.TravelBot.NETCoreReactApp.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KAHA.TravelBot.NETCoreReactApp.Services
{
    public class TravelBotService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TravelBotService> _logger; 

        public TravelBotService(HttpClient httpClient, ILogger<TravelBotService> logger) 
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<CountryModel>> GetAllCountries()
        {
            var apiUrl = "https://restcountries.com/v3.1/all";
            var response = await _httpClient.GetStringAsync(apiUrl);
            var countries = JsonConvert.DeserializeObject<List<CountryModel>>(response);
            return countries;
        }

        // Top 5 Countries by population size
        public async Task<List<CountryModel>> GetTopFiveCountries()
        {
            try
            {
                var allCountries = await GetAllCountries();

                var topFiveCountries = allCountries.OrderByDescending(c => c.Population).Take(5).ToList();

                return topFiveCountries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching top five countries by population");

                return null;
            }
        }

        public async Task<(CountrySummaryModel, string)> GetCountrySummary(string countryName)
        {
            try
            {
                var allCountries = await GetAllCountries();

                // Find the country by name
                var country = allCountries.FirstOrDefault(c => c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase));

                if (country == null)
                {
                    var errorMessage = ("Country not found.");
                    _logger.LogWarning(errorMessage);
                    return (null, errorMessage);
                }

                // Get sunrise and sunset times for the capital city
                var capital = country.Capital;
                if (string.IsNullOrEmpty(capital))
                {
                    var errorMessage = $"Capital not found for '{countryName}'.";
                    _logger.LogWarning(errorMessage);
                    return (null, errorMessage);
                }
                var sunriseSunsetTimes = await GetSunriseSunsetTimes(country.Latitude, country.Longitude);

                if (sunriseSunsetTimes.Item1 == null || sunriseSunsetTimes.Item2 == null)
                {
                    var errorMessage = $"Failed to fetch sunrise and sunset times for '{capital}'.";
                    _logger.LogWarning(errorMessage);
                    return (null, errorMessage);
                }

                var languages = country.Languages;
                if (languages == null)
                {
                    var errorMessage = $"Languages not found for '{countryName}'.";
                    _logger.LogWarning(errorMessage);
                    return (null, errorMessage);
                }

                // Construct the country summary
                var countrySummary = new CountrySummaryModel
                {
                    Name = countryName,
                    Capital = capital,
                    Sunrise = sunriseSunsetTimes.Item1.ToString(),
                    Sunset = sunriseSunsetTimes.Item2.ToString(),
                    OfficialLanguage = languages != null && languages.Any() ? string.Join(", ", languages) : "N/A",
                    TotalLanguages = languages != null ? languages.Count() : 0
                };

                return (countrySummary, null); 
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while getting country summary for '{countryName}': {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (null, errorMessage);
            }
        }
        public async Task<(List<CountrySummaryModel>, string)> GetCountrySummaries(List<string> countryNames)
        {
            var countrySummaries = new List<CountrySummaryModel>();
            var errorMessages = new List<string>();

            try
            {
                foreach (var countryName in countryNames)
                {
                    var (summary, errorMessage) = await GetCountrySummary(countryName);
                    if (summary != null)
                    {
                        countrySummaries.Add(summary);
                    }
                    else
                    {
                        errorMessages.Add(errorMessage);
                    }
                }

                return (countrySummaries, string.Join("; ", errorMessages));
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred while getting country summaries: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                errorMessages.Add(errorMessage);
                return (null, string.Join("; ", errorMessages));
            }
        }

        public async Task<CountryModel> GetRandomCountry()
        {
            try
            {
                var allCountries = await GetAllCountries();

                // Select a random country
                var random = new Random();
                var randomIndex = random.Next(0, allCountries.Count);
                var randomCountry = allCountries[randomIndex];

                return randomCountry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting a random country");
                throw; 
            }
        }


        public CountryModel RandomCountryInSouthernHemisphere(List<CountryModel> countries)
        {
            // Subtle bug in code fix, less than 0 altitude for countries in the Southern Hemisphere
            var countriesInSouthernHemisphere = countries.Where(x => x.Latitude < 0);
            var random = new Random();
            var randomIndex = random.Next(0, countriesInSouthernHemisphere.Count());
            return countriesInSouthernHemisphere.ElementAt(randomIndex);
        }

        public async Task<(DateTime, DateTime)> GetSunriseSunsetTimes(float latitude, float longitude)
        {
            // Construct the API URL with the latitude and longitude parameters
            var apiUrl = $"https://api.sunrise-sunset.org/json?lat={latitude}&lng={longitude}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var status = responseObject["status"].ToString();

                    if (status == "OK")
                    {
                        var results = responseObject["results"];
                        var sunrise = DateTime.Parse(results["sunrise"].ToString());
                        var sunset = DateTime.Parse(results["sunset"].ToString());

                        return (sunrise, sunset);
                    }
                    else
                    {
                        var errorMessage = $"Failed to retrieve sunrise and sunset times. Status: {status}";
                        _logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }
                }
                else
                {
                    var errorMessage = $"Failed to retrieve sunrise and sunset times. Status code: {response.StatusCode}";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
            }
        }
    }
}
