using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Receivers;

namespace AnalyzerApi.Services
{
    public class SlotDataPicker
    {
        private readonly ServerStatusReceiver serverStatusReceiver;
        private readonly ServerLoadReceiver loadReceiver;
        private readonly CustomReceiver customReceiver;

        public SlotDataPicker(ServerStatusReceiver serverStatusReceiver, ServerLoadReceiver loadReceiver, CustomReceiver customReceiver)
        {
            this.serverStatusReceiver = serverStatusReceiver;
            this.loadReceiver = loadReceiver;
            this.customReceiver = customReceiver;
        }

        public SlotData GetSlotData(string key)
        {

        }
    }
}
