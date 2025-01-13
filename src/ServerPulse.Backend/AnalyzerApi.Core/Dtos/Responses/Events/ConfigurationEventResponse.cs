namespace AnalyzerApi.Core.Dtos.Responses.Events
{
    public class ConfigurationEventResponse : BaseEventResponse
    {
        public TimeSpan ServerKeepAliveInterval { get; set; }
    }
}
