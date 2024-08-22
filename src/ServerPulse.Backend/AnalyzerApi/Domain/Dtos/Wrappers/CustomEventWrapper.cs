namespace AnalyzerApi.Domain.Dtos.Wrappers
{
    public class CustomEventWrapper : BaseEventWrapper
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SerializedMessage { get; set; }
    }
}