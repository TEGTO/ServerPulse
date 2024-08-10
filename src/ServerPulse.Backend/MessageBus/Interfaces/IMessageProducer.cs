namespace MessageBus.Interfaces
{
    public interface IMessageProducer
    {
        public Task ProduceAsync(string topic, string message, CancellationToken cancellationToken);
    }
}