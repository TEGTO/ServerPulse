using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Endpoints.Analyze.GetDailyLoadAmountStatistics.Tests
{
    [TestFixture]
    internal class GetDailyLoadAmountStatisticsControllerTests
    {
        private Mock<ILoadAmountStatisticsReceiver> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetDailyLoadAmountStatisticsController handler;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<ILoadAmountStatisticsReceiver>();
            mockMapper = new Mock<IMapper>();

            handler = new GetDailyLoadAmountStatisticsController(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task GetDailyLoadAmountStatistics_ValidQuery_ReturnsMappedResponse()
        {
            // Arrange
            var key = "testKey";

            var timeSpan = TimeSpan.FromDays(1);

            var historicalStatistics = new List<LoadAmountStatistics>
            {
                new LoadAmountStatistics { AmountOfEvents = 10, DateFrom = DateTime.UtcNow.AddHours(-20), DateTo = DateTime.UtcNow.AddDays(-20) },
                new LoadAmountStatistics { AmountOfEvents = 15, DateFrom = DateTime.UtcNow.AddHours(-2), DateTo = DateTime.UtcNow.AddHours(-2) }
            };

            var mappedResponses = new List<LoadAmountStatisticsResponse>
            {
                new LoadAmountStatisticsResponse { AmountOfEvents = 10, DateFrom = historicalStatistics[0].DateFrom, DateTo = historicalStatistics[0].DateTo },
                new LoadAmountStatisticsResponse { AmountOfEvents = 15, DateFrom = historicalStatistics[0].DateFrom, DateTo = historicalStatistics[0].DateTo }
            };

            mockReceiver.Setup(r => r.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(historicalStatistics);
            mockMapper.Setup(m => m.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns((LoadAmountStatistics s) => mappedResponses.First(r => r.AmountOfEvents == s.AmountOfEvents));

            // Act
            var result = await handler.GetDailyLoadAmountStatistics(key, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as IEnumerable<LoadAmountStatisticsResponse>;
            Assert.IsNotNull(response);

            Assert.That(response.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()), Times.Exactly(mappedResponses.Count));
        }
    }
}