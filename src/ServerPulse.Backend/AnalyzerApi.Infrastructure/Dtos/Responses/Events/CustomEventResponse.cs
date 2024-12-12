namespace AnalyzerApi.Infrastructure.Dtos.Responses.Events
{
    public class CustomEventResponse : BaseEventResponse
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? SerializedMessage { get; set; }
    }
}