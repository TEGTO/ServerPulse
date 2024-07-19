
namespace Kafka
{
    public interface IMessageProducer
    {
        public Task ProduceAsync(string topic, string message, int partitionAmount);
    }
}