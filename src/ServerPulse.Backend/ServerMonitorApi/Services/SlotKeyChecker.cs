using Shared;
using Shared.Dtos.ServerSlot;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApi.Services
{
    public class SlotKeyChecker : ISlotKeyChecker
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICacheService cacheService;
        private readonly string slotCheckerUrl;
        private readonly int partitionsAmount;
        private readonly double cacheExpiryInMinutes;

        public SlotKeyChecker(IHttpClientFactory httpClientFactory, ICacheService cacheService, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.cacheService = cacheService;

            slotCheckerUrl = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.SERVER_SLOT_ALIVE_CHECKER]}";
            cacheExpiryInMinutes = double.Parse(configuration[Configuration.CACHE_SERVER_SLOT_EXPIRY_IN_MINUTES]!);
        }

        public async Task<bool> CheckSlotKeyAsync(string slotKey, CancellationToken cancellationToken)
        {
            if (await CheckSlotKeyInCacheAsync(slotKey))
            {
                return true;
            }

            var httpClient = httpClientFactory.CreateClient();
            var checkServerSlotRequest = new CheckSlotKeyRequest()
            {
                SlotKey = slotKey,
            };
            var jsonContent = new StringContent
            (
                JsonSerializer.Serialize(checkServerSlotRequest),
                Encoding.UTF8,
                "application/json"
            );
            var checkUrl = slotCheckerUrl;

            var httpResponseMessage = await httpClient.PostAsync(checkUrl, jsonContent, cancellationToken);
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync<CheckSlotKeyResponse>(contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response != null && response.IsExisting)
            {
                await cacheService.SetValueAsync(slotKey, JsonSerializer.Serialize(response), cacheExpiryInMinutes);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckSlotKeyInCacheAsync(string slotKey)
        {
            var json = await cacheService.GetValueAsync(slotKey);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            if (json.TryToDeserialize(out CheckSlotKeyResponse response))
            {
                return response.IsExisting;
            }
            return false;
        }
    }
}