using ServerSlotApi.Dtos;
using Shared;
using Shared.Helpers;
using System.Text.Json;

namespace ServerMonitorApi.Services
{
    public class SlotKeyChecker : ISlotKeyChecker
    {
        private readonly IHttpHelper httpHelper;
        private readonly ICacheService cacheService;
        private readonly string slotCheckerUrl;
        private readonly double cacheExpiryInMinutes;

        public SlotKeyChecker(IHttpHelper httpHelper, ICacheService cacheService, IConfiguration configuration)
        {
            this.httpHelper = httpHelper;
            this.cacheService = cacheService;

            slotCheckerUrl = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.SERVER_SLOT_ALIVE_CHECKER]}";
            cacheExpiryInMinutes = double.Parse(configuration[Configuration.CACHE_SERVER_SLOT_EXPIRY_IN_MINUTES]!);
        }

        public async Task<bool> CheckSlotKeyAsync(string key, CancellationToken cancellationToken)
        {
            if (await CheckSlotKeyInCacheAsync(key))
            {
                return true;
            }

            var checkServerSlotRequest = new CheckSlotKeyRequest()
            {
                SlotKey = key,
            };

            var response = await httpHelper.SendPostRequestAsync<CheckSlotKeyResponse>(
                slotCheckerUrl,
                JsonSerializer.Serialize(checkServerSlotRequest),
                null,
                cancellationToken
            );

            if (response != null && response.IsExisting)
            {
                await cacheService.SetValueAsync(key, JsonSerializer.Serialize(response), cacheExpiryInMinutes);
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

            if (json.TryToDeserialize(out CheckSlotKeyResponse? response))
            {
                return response?.IsExisting ?? false;
            }

            return false;
        }
    }
}