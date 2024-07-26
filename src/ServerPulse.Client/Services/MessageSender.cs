using System.Text;

namespace ServerPulse.Client.Services
{
    internal sealed class MessageSender : IMessageSender
    {
        private readonly IHttpClientFactory httpClientFactory;

        public MessageSender(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task SendJsonAsync(string json, string url, CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            var jsonContent = new StringContent
            (
                json,
                Encoding.UTF8,
                "application/json"
            );
            var httpResponseMessage = await httpClient.PostAsync(url, jsonContent, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();
        }
    }
}