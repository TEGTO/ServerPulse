using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using ServerMonitorApi;
using ServerMonitorApi.Services;
using Shared.Dtos.ServerSlot;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ServerMonitorApiTests.Services
{
    [TestFixture]
    internal class SlotKeyCheckerTests
    {
        private const string ServerSlotApiUrl = "http://localhost:5000";
        private const double RedisExpiryInMinutes = 60;

        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private Mock<ICacheService> cacheServiceMock;
        private Mock<IConfiguration> configurationMock;
        private SlotKeyChecker slotKeyChecker;

        [SetUp]
        public void SetUp()
        {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            cacheServiceMock = new Mock<ICacheService>();
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(config => config[Configuration.SERVER_SLOT_ALIVE_CHECKER]).Returns(ServerSlotApiUrl);
            configurationMock.Setup(config => config[Configuration.CACHE_SERVER_SLOT_EXPIRY_IN_MINUTES]).Returns(RedisExpiryInMinutes.ToString());

            slotKeyChecker = new SlotKeyChecker(httpClientFactoryMock.Object, cacheServiceMock.Object, configurationMock.Object);
        }

        [Test]
        public async Task CheckSlotKeyAsync_SlotKeyExistsInRedis_ReturnsTrue()
        {
            // Arrange
            var slotKey = "existing-slot-key";
            var redisResponse = new CheckSlotKeyResponse { IsExisting = true };
            var redisJson = JsonSerializer.Serialize(redisResponse);
            cacheServiceMock.Setup(service => service.GetValueAsync(slotKey)).ReturnsAsync(redisJson);
            // Act
            var result = await slotKeyChecker.CheckSlotKeyAsync(slotKey, CancellationToken.None);
            // Assert
            Assert.IsTrue(result);
            cacheServiceMock.Verify(service => service.GetValueAsync(slotKey), Times.Once);
        }
        [Test]
        public async Task CheckSlotKeyAsync_SlotKeyNotInRedisButExistsInServer_ReturnsTrue()
        {
            // Arrange
            var slotKey = "new-slot-key";
            var checkSlotKeyResponse = new CheckSlotKeyResponse { IsExisting = true };
            var responseJson = JsonSerializer.Serialize(checkSlotKeyResponse);
            cacheServiceMock.Setup(service => service.GetValueAsync(slotKey)).ReturnsAsync(string.Empty);
            cacheServiceMock.Setup(service => service.SetValueAsync(slotKey, It.IsAny<string>(), RedisExpiryInMinutes))
                .Returns(Task.CompletedTask);
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Act
            var result = await slotKeyChecker.CheckSlotKeyAsync(slotKey, CancellationToken.None);
            // Assert
            Assert.IsTrue(result);
            cacheServiceMock.Verify(service => service.GetValueAsync(slotKey), Times.Once);
            cacheServiceMock.Verify(service => service.SetValueAsync(slotKey, It.IsAny<string>(), RedisExpiryInMinutes), Times.Once);
        }
        [Test]
        public async Task CheckSlotKeyAsync_SlotKeyNotFound_ReturnsFalse()
        {
            // Arrange
            var slotKey = "non-existing-slot-key";
            cacheServiceMock.Setup(service => service.GetValueAsync(slotKey)).ReturnsAsync(string.Empty);
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new CheckSlotKeyResponse { IsExisting = false }), Encoding.UTF8, "application/json")
                });
            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Act
            var result = await slotKeyChecker.CheckSlotKeyAsync(slotKey, CancellationToken.None);
            // Assert
            Assert.IsFalse(result);
            cacheServiceMock.Verify(service => service.GetValueAsync(slotKey), Times.Once);
            cacheServiceMock.Verify(service => service.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()), Times.Never);
        }
    }
}