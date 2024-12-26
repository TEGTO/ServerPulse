namespace ServerPulse.Client.Services.Interfaces
{
    public interface IQueueMessageSender<in T> where T : class
    {
        public void SendMessage(T message);
    }
}