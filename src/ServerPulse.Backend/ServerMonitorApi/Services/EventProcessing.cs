using ServerPulse.EventCommunication.Events;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.Services
{
    public class EventProcessing : IEventProcessing
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string loadAnalyzeUri;

        public EventProcessing(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            loadAnalyzeUri = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.ANALYZER_LOAD_ANALYZE]}";
        }

        public async Task SendEventsForProcessingsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent
        {
            if (events == null || events.Length == 0)
            {
                return;
            }

            var httpClient = httpClientFactory.CreateClient();

            var jsonContent = new StringContent
            (
                JsonSerializer.Serialize(events),
                Encoding.UTF8,
                "application/json"
            );
            var analyzeUri = GetAnalyzeUri(events.First());
            await httpClient.PostAsync(analyzeUri, jsonContent, cancellationToken);
        }
        private string GetAnalyzeUri(BaseEvent ev)
        {
            return ev switch
            {
                LoadEvent _ => loadAnalyzeUri,
                _ => string.Empty
            };
        }
    }
}