using ServerPulse.Client.Events;
using System.Text;

namespace ServerPulse.Client.Services
{
    internal sealed class EventSender : IEventSender
    {
        private readonly IHttpClientFactory httpClientFactory;

        public EventSender(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task SendEventAsync(BaseEvent @event, string url, CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            var jsonContent = new StringContent
            (
                @event.ToString(),
                Encoding.UTF8,
                "application/json"
            );
            var httpResponseMessage = await httpClient.PostAsync(url, jsonContent, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();
        }
    }
}