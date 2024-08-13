namespace AnalyzerApi.Domain.Dtos.Wrappers
{
    public class LoadEventWrapper : BaseEventWrapper
    {
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime TimestampUTC { get; set; }
    }
}