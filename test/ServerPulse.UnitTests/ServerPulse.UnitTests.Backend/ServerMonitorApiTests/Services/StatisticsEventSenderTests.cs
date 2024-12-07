using Microsoft.Extensions.Configuration;
using Moq;
using ServerPulse.EventCommunication.Events;
using Shared.Helpers;

namespace ServerMonitorApi.Services.Tests
{
    [TestFixture]
    internal class StatisticsEventSenderTests
    {
        private Mock<IHttpHelper> httpHelperMock;
        private StatisticsEventSender statisticsEventSender;
        private string loadAnalyzeUri;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            httpHelperMock = new Mock<IHttpHelper>();

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(config => config[Configuration.API_GATEWAY]).Returns("http://api.gateway/");
            configurationMock.Setup(config => config[Configuration.ANALYZER_LOAD_EVENT]).Returns("analyze/load");

            loadAnalyzeUri = "http://api.gateway/analyze/load";
            statisticsEventSender = new StatisticsEventSender(httpHelperMock.Object, configurationMock.Object);
            cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task SendLoadEventForStatistics_ValidEvent_InvokesHttpHelper()
        {
            // Arrange
            var ev = new LoadEvent("key1", "/endpoint1", "GET", 200, TimeSpan.FromMilliseconds(500), DateTime.UtcNow);

            httpHelperMock
                .Setup(helper => helper.SendPostRequestAsync(It.IsAny<string>(), It.IsAny<string>(), null, cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await statisticsEventSender.SendLoadEventForStatistics(ev, cancellationToken);

            // Assert
            httpHelperMock.Verify(
                helper => helper.SendPostRequestAsync(loadAnalyzeUri, It.IsAny<string>(), null, cancellationToken), Times.Once);
        }
    }
}