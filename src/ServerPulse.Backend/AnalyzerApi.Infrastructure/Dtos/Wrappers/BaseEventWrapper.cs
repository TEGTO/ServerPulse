namespace AnalyzerApi.Infrastructure.Wrappers
{
    public abstract class BaseEventWrapper
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public DateTime CreationDateUTC { get; set; }
    }
}
