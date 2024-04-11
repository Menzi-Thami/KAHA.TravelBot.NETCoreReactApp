using KAHA.TravelBot.NETCoreReactApp.Models;

namespace KAHA.TravelBot.NETCoreReactApp.Services
{
    public interface IWeatherForecastService
    {
        public IEnumerable<WeatherForecast> GetForecast();
    }
}
