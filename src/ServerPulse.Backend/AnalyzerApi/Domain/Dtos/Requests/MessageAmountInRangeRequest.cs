namespace AnalyzerApi.Domain.Dtos.Requests
{
    public class MessageAmountInRangeRequest
    {
        public string Key { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }
}
