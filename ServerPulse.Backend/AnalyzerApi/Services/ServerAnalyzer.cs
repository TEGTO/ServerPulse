using AnalyzerApi.Domain.Models;
using EventCommunication.Events;

namespace AnalyzerApi.Services
{
    public class ServerAnalyzer : IServerAnalyzer
    {
        private readonly IMessageReceiver messageReceiver;

        public ServerAnalyzer(IMessageReceiver messageReceiver)
        {
            this.messageReceiver = messageReceiver;
        }

        public async Task<AnalyzedData> GetAnalyzedDataByKeyAsync(string key)
        {
            AliveEvent aliveEvent = await messageReceiver.ReceiveLastAliveEventByKeyAsync(key);
            AnalyzedData analyzedData = new AnalyzedData
            {
                IsServerAlive = aliveEvent.IsAlive
            };
            return analyzedData;
        }
    }
}