namespace AnalyzerApi.Infrastructure.Requests
{
    public class MessagesInRangeRangeRequest
    {
        public string Key { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}