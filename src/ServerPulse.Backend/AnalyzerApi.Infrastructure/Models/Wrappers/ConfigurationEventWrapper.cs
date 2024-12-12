namespace AnalyzerApi.Infrastructure.Models.Wrappers
{
    public class ConfigurationEventWrapper : BaseEventWrapper
    {
        public TimeSpan ServerKeepAliveInterval { get; set; }
    }
}
