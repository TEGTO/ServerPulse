using MediatR;
using MessageBus.Interfaces;

namespace ServerMonitorApi.Command.DeleteStatisticsByKey
{
    public class DeleteStatisticsByKeyCommandHandler : IRequestHandler<DeleteStatisticsByKeyCommand, Unit>
    {
        private readonly IConfiguration configuration;
        private readonly ITopicManager topicManager;

        public DeleteStatisticsByKeyCommandHandler(ITopicManager topicManager, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.topicManager = topicManager;
        }

        public async Task<Unit> Handle(DeleteStatisticsByKeyCommand command, CancellationToken cancellationToken)
        {
            var key = command.Key;

            var topics = new List<string>
            {
                configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]! + key,
                configuration[Configuration.KAFKA_ALIVE_TOPIC]! + key,
                configuration[Configuration.KAFKA_LOAD_TOPIC]! + key,
                configuration[Configuration.KAFKA_CUSTOM_TOPIC]! + key,
            };

            await topicManager.DeleteTopicsAsync(topics);

            return Unit.Value;
        }
    }
}
