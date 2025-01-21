using ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey;

namespace ServerMonitorApi.Infrastructure.Services
{
    public class SlotKeyChecker : ISlotKeyChecker
    {
        private readonly IServerSlotApi serverSlotApi;

        public SlotKeyChecker(IServerSlotApi serverSlotApi)
        {
            this.serverSlotApi = serverSlotApi;
        }

        public async Task<bool> CheckSlotKeyAsync(string key, CancellationToken cancellationToken)
        {
            var checkServerSlotRequest = new CheckSlotKeyRequest()
            {
                SlotKey = key,
            };

            var response = await serverSlotApi.CheckSlotKeyAsync(checkServerSlotRequest, cancellationToken);

            if (response != null && response.IsExisting)
            {
                return true;
            }

            return false;
        }
    }
}