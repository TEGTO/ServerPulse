namespace AnalyzerApi.Infrastructure.Requests
{
    public class MessageAmountInRangeRequest
    {
        public string Key { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }
}
