using AnalyzerApi;
using AnalyzerApi.Controllers;
using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace AnalyzerApiTests.Controllers
{
    [TestFixture]
    internal class AnalyzeControllerTests
    {
        private Mock<IMapper> mockMapper;
        private Mock<IEventReceiver<LoadEventWrapper>> mockLoadEventReceiver;
        private Mock<IStatisticsReceiver<LoadAmountStatistics>> mockLoadAmountStatisticsReceiver;
        private Mock<IEventReceiver<CustomEventWrapper>> mockCustomEventReceiver;
        private Mock<ICacheService> mockCacheService;
        private Mock<IConfiguration> mockConfiguration;
        private AnalyzeController controller;

        private const string CacheStatisticsKey = "cache-statistics";
        private const double CacheExpiryInMinutes = 60.0;

        [SetUp]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();
            mockLoadEventReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockLoadAmountStatisticsReceiver = new Mock<IStatisticsReceiver<LoadAmountStatistics>>();
            mockCustomEventReceiver = new Mock<IEventReceiver<CustomEventWrapper>>();
            mockCacheService = new Mock<ICacheService>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(config => config[Configuration.CACHE_EXPIRY_IN_MINUTES])
                .Returns(CacheExpiryInMinutes.ToString());
            mockConfiguration.Setup(config => config[Configuration.CACHE_KEY])
                .Returns(CacheStatisticsKey);

            controller = new AnalyzeController(
                mockMapper.Object,
                mockLoadEventReceiver.Object,
                mockLoadAmountStatisticsReceiver.Object,
                mockCustomEventReceiver.Object,
                mockCacheService.Object,
                mockConfiguration.Object
            );
        }

        [Test]
        public async Task GetLoadEventsInDataRange_CacheIsAvailable_ShouldReturnCachedEvents()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest { Key = "test-key", From = DateTime.MinValue, To = DateTime.MaxValue };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-daterange";
            var cachedEvents = new List<LoadEventWrapper> { new LoadEventWrapper() };
            var cachedJson = JsonSerializer.Serialize(cachedEvents);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync(cachedJson);
            // Act
            var result = await controller.GetLoadEventsInDataRange(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var value = okResult?.Value as IEnumerable<LoadEventWrapper>;
            mockCacheService.Verify(x => x.GetValueAsync(cacheKey), Times.Once);
            mockLoadEventReceiver.Verify(x => x.ReceiveEventsInRangeAsync(It.IsAny<InRangeQueryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Test]
        public async Task GetLoadEventsInDataRange_CacheIsNotAvailable_ShouldCallLoadEventReceiverAndCacheResult()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest { Key = "test-key", From = DateTime.MinValue, To = DateTime.MaxValue };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-daterange";
            var events = new List<LoadEventWrapper> { new LoadEventWrapper() };
            var jsonEvents = JsonSerializer.Serialize(events);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync((string)null);
            mockLoadEventReceiver.Setup(x => x.ReceiveEventsInRangeAsync(It.IsAny<InRangeQueryOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            // Act
            var result = await controller.GetLoadEventsInDataRange(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult?.Value, Is.EqualTo(events));
            mockCacheService.Verify(x => x.SetValueAsync(cacheKey, jsonEvents, CacheExpiryInMinutes), Times.Once);
        }
        [Test]
        public async Task GetWholeAmountStatisticsInDays_CacheIsAvailable_ShouldReturnCachedStatistics()
        {
            // Arrange
            var key = "test-key";
            var cacheKey = $"{CacheStatisticsKey}-{key}-perday";
            var cachedStatistics = new List<LoadAmountStatistics> { new LoadAmountStatistics() };
            var cachedJson = JsonSerializer.Serialize(cachedStatistics);
            var loadAmountStatisticsResponse = new LoadAmountStatisticsResponse { AmountOfEvents = 1 };
            mockMapper.Setup(x => x.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns(loadAmountStatisticsResponse);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey))
                .ReturnsAsync(cachedJson);
            // Act
            var result = await controller.GetWholeAmountStatisticsInDays(key, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var value = (okResult?.Value as IEnumerable<LoadAmountStatisticsResponse>)?.ToList();
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Count, Is.EqualTo(1));
            Assert.That(value[0].AmountOfEvents, Is.EqualTo(loadAmountStatisticsResponse.AmountOfEvents));
            mockCacheService.Verify(x => x.GetValueAsync(cacheKey), Times.Once);
            mockLoadAmountStatisticsReceiver.Verify(x => x.GetWholeStatisticsInTimeSpanAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Test]
        public async Task GetWholeAmountStatisticsInDays_CacheIsNotAvailable_ShouldCallLoadAmountStatisticsReceiverAndCacheResult()
        {
            // Arrange
            var key = "test-key";
            var cacheKey = $"{CacheStatisticsKey}-{key}-perday";
            var statistics = new List<LoadAmountStatistics> { new LoadAmountStatistics() };
            var loadAmountStatisticsResponse = new LoadAmountStatisticsResponse { AmountOfEvents = 1 };
            var jsonStatistics = JsonSerializer.Serialize(statistics);
            mockMapper.Setup(x => x.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns(loadAmountStatisticsResponse);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey))
                .ReturnsAsync((string)null);
            mockLoadAmountStatisticsReceiver.Setup(x => x.GetWholeStatisticsInTimeSpanAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(statistics);
            // Act
            var result = await controller.GetWholeAmountStatisticsInDays(key, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var value = (okResult?.Value as IEnumerable<LoadAmountStatisticsResponse>)?.ToList();
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Count, Is.EqualTo(1));
            Assert.That(value[0].AmountOfEvents, Is.EqualTo(loadAmountStatisticsResponse.AmountOfEvents));
            mockCacheService.Verify(x => x.GetValueAsync(cacheKey), Times.Once);
            mockCacheService.Verify(x => x.SetValueAsync(cacheKey, jsonStatistics, CacheExpiryInMinutes), Times.Once);
        }
        [Test]
        public async Task GetAmountStatisticsInRange_CacheIsNotAvailable_ShouldCallLoadAmountStatisticsReceiverAndCacheResult()
        {
            // Arrange
            var request = new MessageAmountInRangeRequest
            {
                Key = "test-key",
                From = DateTime.MinValue,
                To = DateTime.MaxValue,
                TimeSpan = TimeSpan.FromDays(1)
            };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-{request.TimeSpan}-amountrange";
            var statistics = new List<LoadAmountStatistics> { new LoadAmountStatistics { } };
            var loadAmountStatisticsResponse = new LoadAmountStatisticsResponse { AmountOfEvents = 1 };
            var jsonStatistics = JsonSerializer.Serialize(statistics);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync((string)null);
            mockLoadAmountStatisticsReceiver.Setup(x => x.GetStatisticsInRangeAsync(It.IsAny<InRangeQueryOptions>(), request.TimeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(statistics);
            mockMapper.Setup(x => x.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns(loadAmountStatisticsResponse);
            // Act
            var result = await controller.GetAmountStatisticsInRange(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var value = (okResult?.Value as IEnumerable<LoadAmountStatisticsResponse>)?.ToList();
            Assert.That(value[0].AmountOfEvents, Is.EqualTo(loadAmountStatisticsResponse.AmountOfEvents));
            mockCacheService.Verify(x => x.SetValueAsync(cacheKey, jsonStatistics, CacheExpiryInMinutes), Times.Once);
        }
        [Test]
        public async Task GetSomeLoadEvents_ShouldCallLoadEventReceiver()
        {
            // Arrange
            var request = new GetSomeMessagesRequest
            {
                Key = "test-key",
                StartDate = DateTime.MinValue,
                NumberOfMessages = 5,
                ReadNew = true
            };
            var events = new List<LoadEventWrapper> { new LoadEventWrapper { } };
            mockLoadEventReceiver.Setup(x => x.GetCertainAmountOfEventsAsync(It.IsAny<ReadCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            // Act
            var result = await controller.GetSomeLoadEvents(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult?.Value, Is.EqualTo(events));
        }
        [Test]
        public async Task GetSomeCustomEvents_ShouldCallCustomEventReceiver()
        {
            // Arrange
            var request = new GetSomeMessagesRequest
            {
                Key = "test-key",
                StartDate = DateTime.MinValue,
                NumberOfMessages = 5,
                ReadNew = true
            };
            var events = new List<CustomEventWrapper> { new CustomEventWrapper { } };
            mockCustomEventReceiver.Setup(x => x.GetCertainAmountOfEventsAsync(It.IsAny<ReadCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            // Act
            var result = await controller.GetSomeCustomEvents(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult?.Value, Is.EqualTo(events));
        }
    }
}