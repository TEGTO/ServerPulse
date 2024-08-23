namespace AnalyzerApi.Domain.Dtos.Requests
{
    public class MessagesInRangeRangeRequest
    {
        public string Key { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}