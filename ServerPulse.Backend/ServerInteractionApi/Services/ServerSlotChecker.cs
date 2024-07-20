using Shared.Dtos.ServerSlot;
using System.Text;
using System.Text.Json;

namespace ServerInteractionApi.Services
{
    public class ServerSlotChecker : IServerSlotChecker
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IRedisService redisService;
        private readonly IConfiguration configuration;
        private readonly string serverSlotApi;

        public ServerSlotChecker(IHttpClientFactory httpClientFactory, IRedisService redisService, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.redisService = redisService;
            this.configuration = configuration;
            serverSlotApi = configuration[Configuration.SERVER_SLOT_API]!;
        }

        public async Task<bool> CheckServerSlotAsync(string slotKey, CancellationToken cancellationToken)
        {
            if (await CheckSlotInRedisAsync(slotKey))
            {
                return true;
            }

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

            var httpResponseMessage = await httpClient.PostAsJsonAsync(checkUrl, jsonContent, cancellationToken);
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync<CheckServerSlotResponse>(contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response != null && response.IsExisting)
            {
                redisService.SetValueAsync(slotKey, JsonSerializer.Serialize(response));
                return true;
            }

            return false;
        }

        private async Task<bool> CheckSlotInRedisAsync(string slotKey)
        {
            var json = await redisService.GetValueAsync(slotKey);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            var response = JsonSerializer.Deserialize<CheckServerSlotResponse>(json);
            return response.IsExisting;
        }
    }
}