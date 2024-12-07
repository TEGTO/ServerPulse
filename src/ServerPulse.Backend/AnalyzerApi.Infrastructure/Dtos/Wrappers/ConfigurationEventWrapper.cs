namespace AnalyzerApi.Infrastructure.Wrappers
{
    public class ConfigurationEventWrapper : BaseEventWrapper
    {
        public TimeSpan ServerKeepAliveInterval { get; set; }
    }
}
