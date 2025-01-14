namespace AnalyzerApi.Core.Dtos.Responses.Events
{
    public class LoadEventResponse : BaseEventResponse
    {
        public string? Endpoint { get; set; }
        public string? Method { get; set; }
        public int StatusCode { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime TimestampUTC { get; set; }
    }
}