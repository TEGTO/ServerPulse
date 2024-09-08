namespace ServerPulse.Client.Services.Interfaces
{
    public interface IQueueMessageSender<T> where T : class
    {
        public void SendMessage(T ev);
    }
}