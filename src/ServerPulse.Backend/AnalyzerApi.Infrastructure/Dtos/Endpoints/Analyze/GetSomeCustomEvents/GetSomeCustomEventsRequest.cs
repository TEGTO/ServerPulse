namespace AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetSomeCustomEvents
{
    public class GetSomeCustomEventsRequest
    {
        public string Key { get; set; } = string.Empty;
        public int NumberOfMessages { get; set; }
        public DateTime StartDate { get; set; }
        public bool ReadNew { get; set; }
    }
}
