namespace AnalyzerApi.Infrastructure.Models.Wrappers
{
    public class LoadEventWrapper : BaseEventWrapper
    {
        public required string Endpoint { get; set; }
        public required string Method { get; set; }
        public int StatusCode { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime TimestampUTC { get; set; }
    }
}