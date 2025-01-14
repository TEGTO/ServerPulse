namespace AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetSomeLoadEvents
{
    public class GetSomeLoadEventsRequest
    {
        public string Key { get; set; } = string.Empty;
        public int NumberOfMessages { get; set; }
        public DateTime StartDate { get; set; }
        public bool ReadNew { get; set; }
    }
}
