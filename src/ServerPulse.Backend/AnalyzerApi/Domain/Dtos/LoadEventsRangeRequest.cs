namespace AnalyzerApi.Domain.Dtos
{
    public class LoadEventsRangeRequest
    {
        public string Key { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}