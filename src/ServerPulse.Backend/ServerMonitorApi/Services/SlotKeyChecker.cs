using ServerSlotApi.Dtos;
using Shared.Helpers;
using System.Text.Json;

namespace ServerMonitorApi.Services
{
    public class SlotKeyChecker : ISlotKeyChecker
    {
        private readonly IHttpHelper httpHelper;
        private readonly string slotCheckerUrl;

        public SlotKeyChecker(IHttpHelper httpHelper, IConfiguration configuration)
        {
            this.httpHelper = httpHelper;
            slotCheckerUrl = $"{configuration[Configuration.SERVER_SLOT_URL]}{configuration[Configuration.SERVER_SLOT_ALIVE_CHECKER]}";
        }

        public async Task<bool> CheckSlotKeyAsync(string key, CancellationToken cancellationToken)
        {
            var checkServerSlotRequest = new CheckSlotKeyRequest()
            {
                SlotKey = key,
            };

            var response = await httpHelper.SendPostRequestAsync<CheckSlotKeyResponse>(
                slotCheckerUrl,
                JsonSerializer.Serialize(checkServerSlotRequest),
                cancellationToken: cancellationToken
            );

            if (response != null && response.IsExisting)
            {
                return true;
            }

            return false;
        }
    }
}