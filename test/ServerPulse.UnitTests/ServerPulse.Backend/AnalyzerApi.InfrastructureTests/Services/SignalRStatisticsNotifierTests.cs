using AnalyzerApi.Infrastructure.Hubs;
using AnalyzerApi.InfrastructureTests;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AnalyzerApi.Infrastructure.Services.Tests
{
    [TestFixture]
    internal class SignalRStatisticsNotifierTests
    {
        private Mock<IHubContext<StatisticsHub<TestStatistics, TestStatisticsResponse>, IStatisticsHubClient<TestStatisticsResponse>>> hubContextMock;
        private Mock<IStatisticsHubClient<TestStatisticsResponse>> clientProxyMock;
        private Mock<IHubClients<IStatisticsHubClient<TestStatisticsResponse>>> hubClientsMock;

        private SignalRStatisticsNotifier<TestStatistics, TestStatisticsResponse> notifier;

        [SetUp]
        public void SetUp()
        {
            hubContextMock = new Mock<IHubContext<StatisticsHub<TestStatistics, TestStatisticsResponse>, IStatisticsHubClient<TestStatisticsResponse>>>();
            clientProxyMock = new Mock<IStatisticsHubClient<TestStatisticsResponse>>();
            hubClientsMock = new Mock<IHubClients<IStatisticsHubClient<TestStatisticsResponse>>>();

            hubContextMock
                .Setup(h => h.Clients)
                .Returns(hubClientsMock.Object);

            hubClientsMock
                .Setup(c => c.Group(It.IsAny<string>()))
                .Returns(clientProxyMock.Object);

            notifier = new SignalRStatisticsNotifier<TestStatistics, TestStatisticsResponse>(hubContextMock.Object);
        }

        [Test]
        public async Task NotifyGroupAsync_ValidGroupAndData_CallsGroupWithCorrectParameters()
        {
            // Arrange
            var groupKey = "test-group";
            var testData = new TestStatisticsResponse();

            // Act
            await notifier.NotifyGroupAsync(groupKey, testData);

            // Assert
            hubClientsMock.Verify(c => c.Group(groupKey), Times.Once);
            clientProxyMock.Verify(c => c.ReceiveStatistics(groupKey, testData), Times.Once);
        }
    }
}
