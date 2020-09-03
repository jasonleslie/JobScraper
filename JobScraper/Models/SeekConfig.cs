namespace JobScraper.Models
{
    public partial class SeekConfig
    {
        public short Id { get; set; }
        public short DateRange { get; set; }
        public string[] SearchStates { get; set; }
        public string[] ExcludeLocations { get; set; }
    }
}
