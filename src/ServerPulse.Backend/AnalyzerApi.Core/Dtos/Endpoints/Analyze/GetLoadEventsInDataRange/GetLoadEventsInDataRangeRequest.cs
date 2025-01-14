namespace AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange
{
    public class GetLoadEventsInDataRangeRequest
    {
        public string Key { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
