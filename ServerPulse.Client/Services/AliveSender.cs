using Microsoft.Extensions.Hosting;

namespace ServerPulse.Client.Services
{
    internal class AliveSender : BackgroundService
    {
        private readonly IEventSender eventSender;
        private readonly string aliveUrl;

        public AliveSender(IEventSender eventSender, Configuration configuration)
        {
            this.eventSender = eventSender;
            aliveUrl = configuration.EventController + $"/alive/{configuration.SlotKey}";
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
