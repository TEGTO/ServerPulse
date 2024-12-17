namespace MessageBus.Interfaces
{
    public interface IMessageProducer
    {
        public Task ProduceAsync(string topic, string value, CancellationToken cancellationToken);
    }
}