
namespace MessageBus
{
    public interface IMessageProducer
    {
        public Task ProduceAsync(string topic, object objectToMessage, int partitionAmount);
    }
}