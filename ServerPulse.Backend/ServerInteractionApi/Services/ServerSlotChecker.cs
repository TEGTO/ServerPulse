using Microsoft.EntityFrameworkCore.Storage;
using Shared.Dtos.ServerSlot;
using System.Text;
using System.Text.Json;

namespace ServerInteractionApi.Services
{
    public class ServerSlotChecker : IServerSlotChecker
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IDatabase redis;
        private readonly IConfiguration configuration;
        private readonly string serverSlotApi;

        public ServerSlotChecker(IHttpClientFactory httpClientFactory,/* IDatabase redis,*/ IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            //this.redis = redis;
            this.configuration = configuration;
            serverSlotApi = configuration[Configuration.SERVER_SLOT_API];
        }

        public async Task<bool> CheckServerSlotAsync(string slotKey, CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            var checkServerSlotRequest = new CheckServerSlotRequest()
            {
                SlotKey = slotKey,
            };
            var jsonContent = new StringContent
            (
                JsonSerializer.Serialize(checkServerSlotRequest),
                Encoding.UTF8,
                "application/json"
            );
            var checkUrl = serverSlotApi + "/check";
            try
            {
                var httpResponseMessage = await httpClient.PostAsync(checkUrl, jsonContent, cancellationToken);
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                var response = await JsonSerializer.DeserializeAsync<CheckServerSlotResponse>(contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return response.IsExisting;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}