using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi;
using ServerMonitorApi.Services;

namespace ServerMonitorApiTests.Services
{
    [TestFixture]
    internal class StatisticsControlServiceTests
    {
        private Mock<ITopicManager> mockTopicManager;
        private Mock<IConfiguration> mockConfiguration;
        private StatisticsControlService service;

        [SetUp]
        public void Setup()
        {
            mockTopicManager = new Mock<ITopicManager>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c[Configuration.KAFKA_CONFIGURATION_TOPIC]).Returns("conf-topic");
            mockConfiguration.Setup(c => c[Configuration.KAFKA_LOAD_TOPIC]).Returns("load-topic");
            mockConfiguration.Setup(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns("alive-topic");

            service = new StatisticsControlService(mockTopicManager.Object, mockConfiguration.Object);
        }
        [Test]
        public async Task DeleteStatisticsByKeyAsync_ShouldCallDeleteTopicsAsync()
        {
            // Arrange
            var key = "-some-key";
            var topics = new List<string> { "conf-topic-some-key", "alive-topic-some-key", "load-topic-some-key" };
            // Act
            await service.DeleteStatisticsByKeyAsync(key);
            // Assert
            mockTopicManager.Verify(
                tm => tm.DeleteTopicsAsync(topics),
                Times.Once
            );
        }
    }
}