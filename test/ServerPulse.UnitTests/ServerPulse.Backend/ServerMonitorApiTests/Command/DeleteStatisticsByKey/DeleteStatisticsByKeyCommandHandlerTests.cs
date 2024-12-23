﻿using MediatR;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ServerMonitorApi.Command.DeleteStatisticsByKey.Tests
{
    [TestFixture]
    internal class DeleteStatisticsByKeyCommandHandlerTests
    {
        private Mock<ITopicManager> topicManagerMock;
        private Mock<IConfiguration> configurationMock;
        private DeleteStatisticsByKeyCommandHandler handler;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            topicManagerMock = new Mock<ITopicManager>();
            configurationMock = new Mock<IConfiguration>();

            handler = new DeleteStatisticsByKeyCommandHandler(topicManagerMock.Object, configurationMock.Object);
            cancellationToken = CancellationToken.None;
        }

        private static IEnumerable<TestCaseData> DeleteStatisticsTestCases()
        {
            yield return new TestCaseData("key1", new List<string>
            {
                "KafkaConfigurationTopic-key1",
                "KafkaAliveTopic-key1",
                "KafkaLoadTopic-key1",
                "KafkaCustomTopic-key1"
            }).SetDescription("Key 'key1' should create expected topic names.");

            yield return new TestCaseData("testKey", new List<string>
            {
                "KafkaConfigurationTopic-testKey",
                "KafkaAliveTopic-testKey",
                "KafkaLoadTopic-testKey",
                "KafkaCustomTopic-testKey"
            }).SetDescription("Key 'testKey' should create expected topic names.");

            yield return new TestCaseData("", new List<string>
            {
                "KafkaConfigurationTopic-",
                "KafkaAliveTopic-",
                "KafkaLoadTopic-",
                "KafkaCustomTopic-"
            }).SetDescription("Empty key should still create topics with suffixes.");
        }

        [Test]
        [TestCaseSource(nameof(DeleteStatisticsTestCases))]
        public async Task Handle_ValidKey_DeletesCorrectTopics(string key, List<string> expectedTopics)
        {
            // Arrange
            configurationMock.Setup(c => c[Configuration.KAFKA_CONFIGURATION_TOPIC]).Returns("KafkaConfigurationTopic-");
            configurationMock.Setup(c => c[Configuration.KAFKA_ALIVE_TOPIC]).Returns("KafkaAliveTopic-");
            configurationMock.Setup(c => c[Configuration.KAFKA_LOAD_TOPIC]).Returns("KafkaLoadTopic-");
            configurationMock.Setup(c => c[Configuration.KAFKA_CUSTOM_TOPIC]).Returns("KafkaCustomTopic-");

            var command = new DeleteStatisticsByKeyCommand(key);

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(Unit.Value));
            topicManagerMock.Verify(m => m.DeleteTopicsAsync(It.Is<List<string>>(topics =>
                topics.SequenceEqual(expectedTopics))), Times.Once);
        }
    }
}