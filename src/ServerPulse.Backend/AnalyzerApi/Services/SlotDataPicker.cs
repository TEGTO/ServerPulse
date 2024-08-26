using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Receivers;
using AnalyzerApi.Services.Receivers.Event;

namespace AnalyzerApi.Services
{
    public class SlotDataPicker
    {
        private readonly ServerStatusReceiver serverStatusReceiver;
        private readonly ServerLoadReceiver loadReceiver;
        private readonly CustomEventReceiver customReceiver;

        public SlotDataPicker(ServerStatusReceiver serverStatusReceiver, ServerLoadReceiver loadReceiver, CustomEventReceiver customReceiver)
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
