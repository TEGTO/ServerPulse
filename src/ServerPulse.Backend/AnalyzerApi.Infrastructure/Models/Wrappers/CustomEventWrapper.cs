namespace AnalyzerApi.Infrastructure.Models.Wrappers
{
    public class CustomEventWrapper : BaseEventWrapper
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string SerializedMessage { get; set; }
    }
}