using MediatR;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using ServerMonitorApi.Options;

namespace ServerMonitorApi.Command.DeleteStatisticsByKey
{
    public class DeleteStatisticsByKeyCommandHandler : IRequestHandler<DeleteStatisticsByKeyCommand, Unit>
    {
        private readonly MessageBusSettings settings;
        private readonly ITopicManager topicManager;

        public DeleteStatisticsByKeyCommandHandler(ITopicManager topicManager, IOptions<MessageBusSettings> options)
        {
            this.settings = options.Value;
            this.topicManager = topicManager;
        }

        public async Task<Unit> Handle(DeleteStatisticsByKeyCommand command, CancellationToken cancellationToken)
        {
            var key = command.Key;

            var topics = new List<string>
            {
                settings.ConfigurationTopic + key,
                settings.AliveTopic + key,
                settings.LoadTopic + key,
                settings.CustomTopic + key,
            };

            await topicManager.DeleteTopicsAsync(topics);

            return Unit.Value;
        }
    }
}
