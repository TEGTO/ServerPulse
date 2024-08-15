namespace AnalyzerApi.Domain.Dtos.Wrappers
{
    public class PulseEventWrapper : BaseEventWrapper
    {
        public string Id { get; set; }
        public bool IsAlive { get; set; }
    }
}
