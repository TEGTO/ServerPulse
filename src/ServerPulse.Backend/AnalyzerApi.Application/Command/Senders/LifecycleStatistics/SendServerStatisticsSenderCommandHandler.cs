using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Application.Configuration;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AutoMapper;
using MediatR;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AnalyzerApi.Application.Command.Senders.LifecycleStatistics
{
    public sealed class SendServerStatisticsSenderCommandHandler : IRequestHandler<SendStatisticsCommand<ServerLifecycleStatistics>, Unit>
    {
        private readonly IMessageProducer producer;
        private readonly IStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse> notifier;
        private readonly IMapper mapper;
        private readonly string serverStatisticsTopic;

        public SendServerStatisticsSenderCommandHandler(IMessageProducer producer, IStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse> notifier, IMapper mapper, IOptions<MessageBusSettings> options)
        {
            this.producer = producer;
            this.notifier = notifier;
            this.mapper = mapper;
            serverStatisticsTopic = options.Value.ServerStatisticsTopic;
        }

        public async Task<Unit> Handle(SendStatisticsCommand<ServerLifecycleStatistics> command, CancellationToken cancellationToken)
        {
            var topic = serverStatisticsTopic + command.Key;

            await producer.ProduceAsync(topic, JsonSerializer.Serialize(command.Statistics), cancellationToken);

            var response = mapper.Map<ServerLifecycleStatisticsResponse>(command.Statistics);
            await notifier.NotifyGroupAsync(command.Key, response);

            return Unit.Value;
        }
    }
}
