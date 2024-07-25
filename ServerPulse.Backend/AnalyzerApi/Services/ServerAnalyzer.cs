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

        public async Task<ServerStatus> GetCurrentServerStatusByKeyAsync(string key, CancellationToken cancellationToken)
        {
            AliveEvent aliveEvent = await messageReceiver.ReceiveLastAliveEventByKeyAsync(key, cancellationToken);
            ServerStatus analyzedData = new ServerStatus
            {
                IsServerAlive = aliveEvent.IsAlive
            };
            return analyzedData;
        }
    }
}