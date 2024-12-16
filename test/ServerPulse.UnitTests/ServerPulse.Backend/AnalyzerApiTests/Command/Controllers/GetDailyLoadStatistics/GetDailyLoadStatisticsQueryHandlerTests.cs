using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using Moq;
using System.Reflection;

namespace AnalyzerApi.Command.Controllers.GetDailyLoadStatistics.Tests
{
    [TestFixture]
    internal class GetDailyLoadStatisticsQueryHandlerTests
    {
        private Mock<ILoadAmountStatisticsReceiver> mockReceiver;
        private Mock<IMapper> mockMapper;
        private GetDailyLoadStatisticsQueryHandler handler;

        [SetUp]
        public void Setup()
        {
            mockReceiver = new Mock<ILoadAmountStatisticsReceiver>();
            mockMapper = new Mock<IMapper>();

            handler = new GetDailyLoadStatisticsQueryHandler(mockReceiver.Object, mockMapper.Object);
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsMappedResponse()
        {
            // Arrange
            var key = "testKey";
            var query = new GetDailyLoadStatisticsQuery(key);

            var timeSpan = TimeSpan.FromDays(1);

            var historicalStatistics = new List<LoadAmountStatistics>
            {
                new LoadAmountStatistics { AmountOfEvents = 10, DateFrom = DateTime.UtcNow.AddHours(-20), DateTo = DateTime.UtcNow.AddDays(-20) },
                new LoadAmountStatistics { AmountOfEvents = 15, DateFrom = DateTime.UtcNow.AddHours(-2), DateTo = DateTime.UtcNow.AddHours(-2) } //Must be ignored
            };

            var todayStatistics = new List<LoadAmountStatistics>
            {
                new LoadAmountStatistics { AmountOfEvents = 20, DateFrom = DateTime.UtcNow.AddHours(-12), DateTo = DateTime.UtcNow }
            };

            var combinedStatistics = new List<LoadAmountStatistics>
            {
                historicalStatistics[0],
                todayStatistics[0]
            };

            var mappedResponses = new List<LoadAmountStatisticsResponse>
            {
                new LoadAmountStatisticsResponse { AmountOfEvents = 10, DateFrom = historicalStatistics[0].DateFrom, DateTo = historicalStatistics[0].DateTo },
                new LoadAmountStatisticsResponse { AmountOfEvents = 20, DateFrom = todayStatistics[0].DateFrom, DateTo = todayStatistics[0].DateTo }
            };

            mockReceiver.Setup(r => r.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(historicalStatistics);
            mockReceiver.Setup(r => r.GetStatisticsInRangeAsync(It.IsAny<GetInRangeOptions>(), timeSpan, It.IsAny<CancellationToken>()))
                .ReturnsAsync(todayStatistics);
            mockMapper.Setup(m => m.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()))
                .Returns((LoadAmountStatistics s) => mappedResponses.First(r => r.AmountOfEvents == s.AmountOfEvents));

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(mappedResponses.Count));

            mockReceiver.Verify(r => r.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, It.IsAny<CancellationToken>()), Times.Once);
            mockReceiver.Verify(r => r.GetStatisticsInRangeAsync(It.IsAny<GetInRangeOptions>(), timeSpan, It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<LoadAmountStatisticsResponse>(It.IsAny<LoadAmountStatistics>()), Times.Exactly(combinedStatistics.Count));
        }

        [Test]
        [TestCase(
            new[] { "2023-12-01T00:00:00,2023-12-01T12:00:00" },
            new[] { "2023-12-02T00:00:00,2023-12-02T12:00:00" },
            2,
            Description = "Non-overlapping intervals"
        )]
        [TestCase(
            new[] { "2023-12-01T00:00:00,2023-12-02T00:00:00" },
            new[] { "2023-12-01T12:00:00,2023-12-02T12:00:00" },
            1,
            Description = "Partially overlapping intervals"
        )]
        [TestCase(
            new[] { "2023-12-01T00:00:00,2023-12-01T23:59:59" },
            new[] { "2023-12-01T12:00:00,2023-12-02T00:00:00" },
            1,
            Description = "Fully overlapping intervals"
        )]
        [TestCase(
            new[] { "2023-12-01T00:00:00,2023-12-01T12:00:00" },
            new[] { "2023-12-02T00:00:00,2023-12-02T12:00:00" },
            2,
            Description = "Disjoint intervals"
        )]
        [TestCase(
            new string[] { },
            new[] { "2023-12-02T00:00:00,2023-12-02T12:00:00" },
            1,
            Description = "Empty historical statistics"
        )]
        [TestCase(
            new[] { "2023-12-01T00:00:00,2023-12-01T12:00:00" },
            new string[] { },
            1,
            Description = "Empty today's statistics"
        )]
        [TestCase(
            new[] { "2023-12-01T00:00:00,2023-12-01T12:00:00" },
            new[] { "2023-12-01T12:00:00,2023-12-02T00:00:00" },
            2,
            Description = "Edge case: intervals meet but do not overlap"
        )]
        public void MergeStatisticsCollectionsTests(string[] historicalInput, string[] todayInput, int expectedCount)
        {
            // Arrange
            var historical = ParseStatistics(historicalInput);
            var today = ParseStatistics(todayInput);

            // Act
            var method = handler.GetType().GetMethod("MergeStatisticsCollections", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (IEnumerable<LoadAmountStatistics>)method!.Invoke(null, new object[] { historical, today })!;

            // Assert
            Assert.That(result.Count(), Is.EqualTo(expectedCount));
        }

        private static IEnumerable<LoadAmountStatistics> ParseStatistics(string[] inputs)
        {
            return inputs.Select(input =>
            {
                var dates = input.Split(',').Select(DateTime.Parse).ToArray();
                return new LoadAmountStatistics
                {
                    DateFrom = dates[0],
                    DateTo = dates[1]
                };
            });
        }
    }
}