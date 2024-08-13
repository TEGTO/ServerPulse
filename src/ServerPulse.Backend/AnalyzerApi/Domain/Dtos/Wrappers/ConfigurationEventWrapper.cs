namespace AnalyzerApi.Domain.Dtos.Wrappers
{
    public class ConfigurationEventWrapper : BaseEventWrapper
    {
        public TimeSpan ServerKeepAliveInterval { get; set; }
    }
}
