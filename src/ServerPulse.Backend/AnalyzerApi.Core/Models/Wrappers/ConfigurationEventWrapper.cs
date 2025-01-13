namespace AnalyzerApi.Core.Models.Wrappers
{
    public class ConfigurationEventWrapper : BaseEventWrapper
    {
        public TimeSpan ServerKeepAliveInterval { get; set; }
    }
}
