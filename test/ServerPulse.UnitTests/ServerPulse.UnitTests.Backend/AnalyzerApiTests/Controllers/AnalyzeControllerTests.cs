using AnalyzerApi;
using AnalyzerApi.Controllers;
using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
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
        private Mock<IServerLoadReceiver> mockServerLoadReceiver;
        private Mock<ICacheService> mockCacheService;
        private Mock<IConfiguration> mockConfiguration;
        private AnalyzeController controller;

        private const string CacheStatisticsKey = "cache-statistics";
        private const double CacheExpiryInMinutes = 60.0;

        [SetUp]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();
            mockServerLoadReceiver = new Mock<IServerLoadReceiver>();
            mockCacheService = new Mock<ICacheService>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration
                .Setup(config => config[Configuration.CACHE_SERVER_LOAD_STATISTICS_PER_DAY_EXPIRY_IN_MINUTES])
                .Returns(CacheExpiryInMinutes.ToString());

            mockConfiguration
                .Setup(config => config[Configuration.CACHE_STATISTICS_KEY])
                .Returns(CacheStatisticsKey);

            controller = new AnalyzeController(
                mockMapper.Object,
                mockServerLoadReceiver.Object,
                mockCacheService.Object,
                mockConfiguration.Object);
        }

        [Test]
        public async Task GetLoadEventsInDataRange_CacheIsAvailable_ShouldReturnCachedEvents()
        {
            // Arrange
            var request = new LoadEventsRangeRequest { Key = "test-key", From = DateTime.MinValue, To = DateTime.MaxValue };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-daterange";
            var cachedEvents = new List<LoadEventWrapper> { new LoadEventWrapper() };
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey))
                .ReturnsAsync(JsonSerializer.Serialize(cachedEvents));
            // Act
            var result = await controller.GetLoadEventsInDataRange(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var value = okResult?.Value as List<LoadEventWrapper>;
            Assert.That(value[0].Key, Is.EqualTo(cachedEvents[0].Key));
            mockCacheService.Verify(x => x.GetValueAsync(cacheKey), Times.Once);
            mockServerLoadReceiver.Verify(x => x.ReceiveEventsInRangeAsync(It.IsAny<InRangeQueryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Test]
        public async Task GetLoadEventsInDataRange_CacheIsNotAvailable_ShouldCallServerLoadReceiverAndCacheResult()
        {
            // Arrange
            var request = new LoadEventsRangeRequest { Key = "test-key", From = DateTime.MinValue, To = DateTime.MaxValue };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-daterange";
            var events = new List<LoadEventWrapper> { new LoadEventWrapper { } };
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync((string)null);
            mockServerLoadReceiver.Setup(x => x.ReceiveEventsInRangeAsync(It.IsAny<InRangeQueryOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            // Act
            var result = await controller.GetLoadEventsInDataRange(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult?.Value, Is.EqualTo(events));
            mockCacheService.Verify(x => x.SetValueAsync(cacheKey, It.IsAny<string>(), CacheExpiryInMinutes), Times.Once);
        }
        [Test]
        public async Task GetWholeAmountStatisticsInDays_CacheIsAvailable_ShouldReturnCachedStatistics()
        {
            // Arrange
            var key = "test-key";
            var cacheKey = $"{CacheStatisticsKey}-{key}-perday";
            var cachedStatistics = new List<LoadAmountStatistics> { new LoadAmountStatistics() };
            var loadAmountStatisticsResponse = new LoadAmountStatisticsResponse { AmountOfEvents = 1 };
            mockMapper.Setup(x => x.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns(loadAmountStatisticsResponse);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey))
                .ReturnsAsync(JsonSerializer.Serialize(cachedStatistics));
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
            mockServerLoadReceiver.Verify(x => x.GetAmountStatisticsInDaysAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Test]
        public async Task GetWholeAmountStatisticsInDays_CacheIsNotAvailable_ShouldCallServerLoadReceiverAndCacheResult()
        {
            // Arrange
            var key = "test-key";
            var cacheKey = $"{CacheStatisticsKey}-{key}-perday";
            var cachedStatistics = new List<LoadAmountStatistics> { new LoadAmountStatistics() };
            var loadAmountStatisticsResponse = new LoadAmountStatisticsResponse { AmountOfEvents = 1 };
            mockMapper.Setup(x => x.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns(loadAmountStatisticsResponse);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey))
                .ReturnsAsync((string)null);
            mockServerLoadReceiver.Setup(x => x.GetAmountStatisticsInDaysAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(cachedStatistics);
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
        }
        [Test]
        public async Task GetAmountStatisticsInRange_CacheIsNotAvailable_ShouldCallServerLoadReceiverAndCacheResult()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = "test-key",
                From = DateTime.MinValue,
                To = DateTime.MaxValue,
                TimeSpan = TimeSpan.FromDays(1)
            };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-{request.TimeSpan}-amountrange";
            var statistics = new List<LoadAmountStatistics> { new LoadAmountStatistics { } };
            var loadAmountStatisticsResponse = new LoadAmountStatisticsResponse { AmountOfEvents = 1 };
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync((string)null);
            mockServerLoadReceiver.Setup(x => x.GetAmountStatisticsInRangeAsync(It.IsAny<InRangeQueryOptions>(), request.TimeSpan, It.IsAny<CancellationToken>()))
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
            mockCacheService.Verify(x => x.SetValueAsync(cacheKey, It.IsAny<string>(), CacheExpiryInMinutes), Times.Once);
        }
        [Test]
        public async Task GetSomeLoadEvents_CacheIsAvailable_ShouldReturnCachedEvents()
        {
            // Arrange
            var request = new GetSomeLoadEventsRequest
            {
                Key = "test-key",
                StartDate = DateTime.MinValue,
                NumberOfMessages = 5,
                ReadNew = true
            };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.StartDate.ToUniversalTime()}-{request.NumberOfMessages}-{request.ReadNew}-someevents";
            var cachedEvents = new List<LoadEventWrapper> { new LoadEventWrapper { Key = "test-key" } };
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey))
                .ReturnsAsync(JsonSerializer.Serialize(cachedEvents));
            // Act
            var result = await controller.GetSomeLoadEvents(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var value = (okResult?.Value as IEnumerable<LoadEventWrapper>)?.ToList();
            Assert.That(value[0].Key, Is.EqualTo(cachedEvents[0].Key));
            mockServerLoadReceiver.Verify(x => x.GetCertainAmountOfEvents(It.IsAny<ReadCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Test]
        public async Task GetSomeLoadEvents_CacheIsNotAvailable_ShouldCallServerLoadReceiverAndCacheResult()
        {
            // Arrange
            var request = new GetSomeLoadEventsRequest
            {
                Key = "test-key",
                StartDate = DateTime.MinValue,
                NumberOfMessages = 5,
                ReadNew = true
            };
            var cacheKey = $"{CacheStatisticsKey}-{request.Key}-{request.StartDate.ToUniversalTime()}-{request.NumberOfMessages}-{request.ReadNew}-someevents";
            var events = new List<LoadEventWrapper> { new LoadEventWrapper { } };
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync((string)null);
            mockServerLoadReceiver.Setup(x => x.GetCertainAmountOfEvents(It.IsAny<ReadCertainMessageNumberOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            // Act
            var result = await controller.GetSomeLoadEvents(request, CancellationToken.None);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult?.Value, Is.EqualTo(events));
            mockCacheService.Verify(x => x.SetValueAsync(cacheKey, It.IsAny<string>(), CacheExpiryInMinutes), Times.Once);
        }
    }
}