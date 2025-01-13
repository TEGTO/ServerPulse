namespace AnalyzerApi.Core.Dtos.Responses.Events
{
    public abstract class BaseEventResponse
    {
        public string? Id { get; set; }
        public string? Key { get; set; }
        public DateTime CreationDateUTC { get; set; }
    }
}
