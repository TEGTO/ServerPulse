using Moq;
using ServerMonitorApi.Services;
using StackExchange.Redis;

namespace ServerMonitorApiTests.Services
{
    [TestFixture]
    internal class RedisServiceTests
    {
        private Mock<IConnectionMultiplexer> connectionMultiplexerMock;
        private Mock<IDatabase> databaseMock;
        private RedisService redisService;

        [SetUp]
        public void SetUp()
        {
            connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
            databaseMock = new Mock<IDatabase>();
            connectionMultiplexerMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(databaseMock.Object);
            redisService = new RedisService(connectionMultiplexerMock.Object);
        }

        [Test]
        public async Task SetValueAsync_ValidKeyAndValue_SetsValueInRedis()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            var expiryInMinutes = 10.0;
            // Act
            await redisService.SetValueAsync(key, value, expiryInMinutes);
            // Assert
            databaseMock.Verify(db => db.StringSetAsync(key, value, TimeSpan.FromMinutes(expiryInMinutes), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        }
        [Test]
        public async Task GetValueAsync_ValidKey_ReturnsValueFromRedis()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = "test-value";
            databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(expectedValue);
            // Act
            var result = await redisService.GetValueAsync(key);
            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            databaseMock.Verify(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()), Times.Once);
        }
        [Test]
        public async Task RemoveKeyAsync_ValidKey_DeletesKeyFromRedis()
        {
            // Arrange
            var key = "test-key";
            databaseMock.Setup(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(true);
            // Act
            var result = await redisService.RemoveKeyAsync(key);
            // Assert
            Assert.IsTrue(result);
            databaseMock.Verify(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
        }
        [Test]
        public async Task RemoveKeyAsync_KeyDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var key = "non-existing-key";
            databaseMock.Setup(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(false);
            // Act
            var result = await redisService.RemoveKeyAsync(key);
            // Assert
            Assert.IsFalse(result);
            databaseMock.Verify(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
        }
    }
}