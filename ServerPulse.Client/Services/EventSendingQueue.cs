namespace ServerPulse.Client.Services
{
    public class EventSendingQueue
    {
        private readonly IEventSender eventSender;
        private readonly Configuration configuration;

        public EventSendingQueue(IEventSender eventSender, Configuration configuration)
        {
            this.eventSender = eventSender;
            this.configuration = configuration;
        }
        //TODO: Create a service that contains a queue of event messages and sends a certain number of them at a certain interval.
        private async Task SendMessage()
        {

        }
    }
}
