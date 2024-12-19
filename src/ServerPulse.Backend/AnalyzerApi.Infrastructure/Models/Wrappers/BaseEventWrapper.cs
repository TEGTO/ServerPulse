namespace AnalyzerApi.Infrastructure.Models.Wrappers
{
    public abstract class BaseEventWrapper
    {
        public required string Id { get; set; }
        public required string Key { get; set; }
        public DateTime CreationDateUTC { get; set; }
    }
}
