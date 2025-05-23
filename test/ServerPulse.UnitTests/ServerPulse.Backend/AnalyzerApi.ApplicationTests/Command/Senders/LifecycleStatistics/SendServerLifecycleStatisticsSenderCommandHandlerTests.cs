﻿using AnalyzerApi.Application.Configuration;
using AnalyzerApi.Application.Services;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Application.Command.Senders.LifecycleStatistics.Tests
{
    [TestFixture]
    internal class SendServerLifecycleStatisticsSenderCommandHandlerTests
    {
        private Mock<IMessageProducer> mockProducer;
        private Mock<IStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>> mockNotifier;
        private Mock<IMapper> mockMapper;
        private SendServerStatisticsSenderCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockProducer = new Mock<IMessageProducer>();
            mockNotifier = new Mock<IStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>>();
            mockMapper = new Mock<IMapper>();

            var settings = new MessageBusSettings
            {
                ServerStatisticsTopic = "server-statistics-topic-"
            };

            var mockOptions = new Mock<IOptions<MessageBusSettings>>();
            mockOptions.Setup(x => x.Value).Returns(settings);

            handler = new SendServerStatisticsSenderCommandHandler(
                mockProducer.Object,
                mockNotifier.Object,
                mockMapper.Object,
                mockOptions.Object);
        }

        [Test]
        public async Task Handle_ValidCommand_SendsToKafkaAndGroup()
        {
            // Arrange
            var key = "testKey";
            var statistics = new ServerLifecycleStatistics
            {
                IsAlive = true,
                DataExists = true,
                ServerLastStartDateTimeUTC = DateTime.UtcNow.AddHours(-2),
                ServerUptime = TimeSpan.FromHours(2),
                LastServerUptime = TimeSpan.FromHours(1),
                LastPulseDateTimeUTC = DateTime.UtcNow
            };

            var response = new ServerLifecycleStatisticsResponse
            {
                IsAlive = statistics.IsAlive,
                DataExists = statistics.DataExists,
                ServerLastStartDateTimeUTC = statistics.ServerLastStartDateTimeUTC,
                ServerUptime = statistics.ServerUptime,
                LastServerUptime = statistics.LastServerUptime,
                LastPulseDateTimeUTC = statistics.LastPulseDateTimeUTC
            };

            mockMapper.Setup(m => m.Map<ServerLifecycleStatisticsResponse>(statistics)).Returns(response);

            // Act
            await handler.Handle(new SendStatisticsCommand<ServerLifecycleStatistics>(key, statistics), CancellationToken.None);

            // Assert
            var topic = "server-statistics-topic-" + key;
            var serializedStatistics = JsonSerializer.Serialize(statistics);

            mockMapper.Verify(m => m.Map<ServerLifecycleStatisticsResponse>(statistics), Times.Once);
            mockProducer.Verify(p => p.ProduceAsync(topic, serializedStatistics, It.IsAny<CancellationToken>()), Times.Once);
            mockNotifier.Verify(c => c.NotifyGroupAsync(key, response), Times.Once);
        }
    }
}