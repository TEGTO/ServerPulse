using Shared.Dtos.ServerSlot;
using System.Text.Json;
using TestKafka.Consumer.Services;

namespace ServerInteractionApi.Services
{
    public class SlotKeyRedisDelete : BackgroundService
    {
        private readonly IRedisService redisService;
        private readonly IMessageConsumer consumer;
        private readonly string deleteTopic;
        private readonly int timeoutInMilliseconds;

        public SlotKeyRedisDelete(IRedisService redisService, IConfiguration configuration, IMessageConsumer consumer)
        {
            this.redisService = redisService;
            this.consumer = consumer;
            deleteTopic = configuration[Configuration.KAFKA_KEY_DELETE_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var consumeResult in consumer.ConsumeAsync(deleteTopic, timeoutInMilliseconds, stoppingToken))
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    var request = JsonSerializer.Deserialize<SlotKeyDeletionRequest>(consumeResult);
                    if (request != null)
                    {
                        await redisService.RemoveKeyAsync(request.SlotKey);
                    }
                }
            }
        }
    }
}