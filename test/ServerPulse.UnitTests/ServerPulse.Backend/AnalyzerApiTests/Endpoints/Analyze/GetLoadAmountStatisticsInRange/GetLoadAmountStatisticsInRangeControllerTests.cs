using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetLoadAmountStatisticsInRange;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApi.Endpoints.Analyze.GetLoadAmountStatisticsInRange.Tests
{
    [TestFixture]
    internal class GetLoadAmountStatisticsInRangeControllerTests
    {
        private Mock<ILoadAmountStatisticsReceiver> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetLoadAmountStatisticsInRangeController controller;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<ILoadAmountStatisticsReceiver>();
            mockMapper = new Mock<IMapper>();

            controller = new GetLoadAmountStatisticsInRangeController(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task GetLoadAmountStatisticsInRange_ValidQuery_ReturnsMappedResponses()
        {
            // Arrange
            var key = "testKey";
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var timeSpan = TimeSpan.FromHours(1);
            var request = new GetLoadAmountStatisticsInRangeRequest
            {
                Key = key,
                From = from,
                To = to,
                TimeSpan = timeSpan
            };

            var statistics = new List<LoadAmountStatistics>
            {
                new LoadAmountStatistics { AmountOfEvents = 10, DateFrom = from, DateTo = to },
                new LoadAmountStatistics { AmountOfEvents = 20, DateFrom = from, DateTo = to }
            };

            var mappedResponses = new List<LoadAmountStatisticsResponse>
            {
                new LoadAmountStatisticsResponse { AmountOfEvents = 10, DateFrom = from, DateTo = to },
                new LoadAmountStatisticsResponse { AmountOfEvents = 20, DateFrom = from, DateTo = to }
            };

            mockReceiver.Setup(r => r.GetStatisticsInRangeAsync(It.IsAny<GetInRangeOptions>(), timeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(statistics);

            mockMapper.Setup(m => m.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns((LoadAmountStatistics s) => mappedResponses.First(r => r.AmountOfEvents == s.AmountOfEvents));

            // Act
            var result = await controller.GetLoadAmountStatisticsInRange(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as IEnumerable<LoadAmountStatisticsResponse>;
            Assert.IsNotNull(response);

            Assert.That(response.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetStatisticsInRangeAsync(It.IsAny<GetInRangeOptions>(), timeSpan, It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()), Times.Exactly(statistics.Count));
        }
    }
}