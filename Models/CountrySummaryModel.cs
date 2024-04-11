namespace KAHA.TravelBot.NETCoreReactApp.Models
{
    // TODO: CountrySummaryModel to be implemented
    public class CountrySummaryModel : CountryModel
    {
        public string Name { get; set; }
        public string Capital { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string OfficialLanguage { get; set; }
        public string DriveSide { get; set; }

        public int TotalLanguages { get; set; }
    }
}
