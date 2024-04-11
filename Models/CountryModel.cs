namespace KAHA.TravelBot.NETCoreReactApp.Models
{
    public class CountryModel
    {
        public string Name { get; set; }
        public string Capital { get; set; }
        public int Population { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public object?[] Languages { get; internal set; }
    }
}
