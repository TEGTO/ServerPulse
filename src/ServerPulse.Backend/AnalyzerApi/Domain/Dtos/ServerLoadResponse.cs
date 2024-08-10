namespace AnalyzerApi.Domain.Dtos
{
    public class ServerLoadResponse
    {
        public string Id { get; set; }
        public DateTime CreationDateUTC { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public int StatusCode { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime TimestampUTC { get; set; }
    }
}