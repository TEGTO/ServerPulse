﻿using AnalyzerApi.Command.Builders.CustomStatistics;
using AnalyzerApi.Command.Builders.LifecycleStatistics;
using AnalyzerApi.Command.Builders.LoadStatistics;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApi.Command.Controllers.GetSlotStatistics.Tests
{
    [TestFixture]
    internal class GetSlotStatisticsQueryHandlerTests
    {
        private Mock<IMediator> mockMediator;
        private Mock<IEventReceiver<LoadEventWrapper>> mockLoadEventReceiver;
        private Mock<IEventReceiver<CustomEventWrapper>> mockCustomEventReceiver;
        private Mock<IMapper> mockMapper;
        private Mock<IConfiguration> mockConfiguration;
        private GetSlotStatisticsQueryHandler handler;

        [SetUp]
        public void Setup()
        {
            mockMediator = new Mock<IMediator>();
            mockLoadEventReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockCustomEventReceiver = new Mock<IEventReceiver<CustomEventWrapper>>();
            mockMapper = new Mock<IMapper>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(c => c[Configuration.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA])
                .Returns("10");

            handler = new GetSlotStatisticsQueryHandler(
                mockMediator.Object,
                mockLoadEventReceiver.Object,
                mockCustomEventReceiver.Object,
                mockMapper.Object,
                mockConfiguration.Object
            );
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsSlotStatisticsResponse()
        {
            // Arrange
            var key = "testKey";
            var query = new GetSlotStatisticsQuery(key);

            var lifecycleStats = new ServerLifecycleStatistics
            {
                IsAlive = true,
                DataExists = true,
                ServerLastStartDateTimeUTC = DateTime.UtcNow.AddDays(-1),
                ServerUptime = TimeSpan.FromHours(10)
            };
            var loadStats = new ServerLoadStatistics
            {
                AmountOfEvents = 5,
                LastEvent = new LoadEventWrapper { Id = "1", Key = key, Endpoint = "/api/test1", Method = "GET", StatusCode = 200, Duration = TimeSpan.FromSeconds(1) }
            };
            var customStats = new ServerCustomStatistics
            {
                LastEvent = new CustomEventWrapper { Id = "1", Key = key, Name = "CustomEvent1", Description = "Test description", SerializedMessage = "" }
            };
            var loadEvents = new List<LoadEventWrapper>
            {
                new LoadEventWrapper {Id = "1", Key = key, Endpoint = "/api/test1", Method = "GET", StatusCode = 200, Duration = TimeSpan.FromSeconds(1)}
            };
            var customEvents = new List<CustomEventWrapper>
            {
                new CustomEventWrapper {Id = "1", Key = key, Name = "CustomEvent1", Description = "Test description", SerializedMessage = ""}
            };

            mockMediator.Setup(m => m.Send(It.IsAny<BuildLifecycleStatisticsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lifecycleStats);
            mockMediator.Setup(m => m.Send(It.IsAny<BuildLoadStatisticsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(loadStats);
            mockMediator.Setup(m => m.Send(It.IsAny<BuildCustomStatisticsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(customStats);

            mockLoadEventReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(loadEvents);
            mockCustomEventReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(customEvents);

            mockMapper.Setup(m => m.Map<ServerLifecycleStatisticsResponse>(lifecycleStats))
                .Returns(new ServerLifecycleStatisticsResponse
                {
                    IsAlive = lifecycleStats.IsAlive,
                    DataExists = lifecycleStats.DataExists,
                    ServerLastStartDateTimeUTC = lifecycleStats.ServerLastStartDateTimeUTC,
                    ServerUptime = lifecycleStats.ServerUptime
                });
            mockMapper.Setup(m => m.Map<ServerLoadStatisticsResponse>(loadStats))
                .Returns(new ServerLoadStatisticsResponse
                {
                    AmountOfEvents = loadStats.AmountOfEvents,
                    LastEvent = new LoadEventResponse
                    {
                        Endpoint = loadStats.LastEvent.Endpoint,
                        Method = loadStats.LastEvent.Method,
                        StatusCode = loadStats.LastEvent.StatusCode,
                        Duration = loadStats.LastEvent.Duration
                    }
                });
            mockMapper.Setup(m => m.Map<ServerCustomStatisticsResponse>(customStats))
                .Returns(new ServerCustomStatisticsResponse
                {
                    LastEvent = new CustomEventResponse
                    {
                        Name = customStats.LastEvent.Name,
                        Description = customStats.LastEvent.Description
                    }
                });
            mockMapper.Setup(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()))
                .Returns(new LoadEventResponse
                {
                    Endpoint = "/api/test1",
                    Method = "GET",
                    StatusCode = 200,
                    Duration = TimeSpan.FromSeconds(1)
                });
            mockMapper.Setup(m => m.Map<CustomEventResponse>(It.IsAny<CustomEventWrapper>()))
                .Returns(new CustomEventResponse
                {
                    Name = "CustomEvent1",
                    Description = "Test description"
                });

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.GeneralStatistics!.IsAlive);
            Assert.That(result.LoadStatistics!.AmountOfEvents, Is.EqualTo(5));
            Assert.That(result.LastLoadEvents.Count(), Is.EqualTo(1));
            Assert.That(result.LastCustomEvents.Count(), Is.EqualTo(1));

            mockMediator.Verify(m => m.Send(It.IsAny<BuildLifecycleStatisticsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(m => m.Send(It.IsAny<BuildLoadStatisticsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(m => m.Send(It.IsAny<BuildCustomStatisticsCommand>(), It.IsAny<CancellationToken>()), Times.Once);

            mockLoadEventReceiver.Verify(m => m.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockCustomEventReceiver.Verify(m => m.GetCertainAmountOfEventsAsync(It.IsAny<GetCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()), Times.Once);

            mockMapper.Verify(m => m.Map<ServerLifecycleStatisticsResponse>(It.IsAny<ServerLifecycleStatistics>()), Times.Once);
            mockMapper.Verify(m => m.Map<ServerLoadStatisticsResponse>(It.IsAny<ServerLoadStatistics>()), Times.Once);
            mockMapper.Verify(m => m.Map<ServerCustomStatisticsResponse>(It.IsAny<ServerCustomStatistics>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadEventResponse>(It.IsAny<LoadEventWrapper>()), Times.Exactly(loadEvents.Count));
            mockMapper.Verify(m => m.Map<CustomEventResponse>(It.IsAny<CustomEventWrapper>()), Times.Exactly(customEvents.Count));
        }
    }
}