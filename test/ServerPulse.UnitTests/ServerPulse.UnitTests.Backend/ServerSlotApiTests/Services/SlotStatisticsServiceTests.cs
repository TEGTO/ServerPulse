using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;

namespace ServerSlotApi.Services.Tests
{
    [TestFixture()]
    public class SlotStatisticsServiceTests
    {
        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private Mock<IConfiguration> configurationMock;
        private SlotStatisticsService slotStatisticsService;
        private string deleteStatisticsUrl;

        [SetUp]
        public void SetUp()
        {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x[Configuration.API_GATEWAY]).Returns("https://api.gateway/");
            configurationMock.Setup(x => x[Configuration.STATISTICS_DELETE_URL]).Returns("delete/statistics/");
            deleteStatisticsUrl = "https://api.gateway/delete/statistics/";
            slotStatisticsService = new SlotStatisticsService(httpClientFactoryMock.Object, configurationMock.Object);
        }

        [Test]
        public async Task DeleteSlotStatisticsAsync_SuccessfulDeletion_ReturnsTrue()
        {
            // Arrange
            var key = "test-key";
            var token = "valid-token";
            var cancellationToken = new CancellationToken();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri == new Uri(deleteStatisticsUrl + key) &&
                        req.Headers.Authorization.Parameter == token),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Act
            var result = await slotStatisticsService.DeleteSlotStatisticsAsync(key, token, cancellationToken);
            // Assert
            Assert.IsTrue(result);
        }
        [Test]
        public async Task DeleteSlotStatisticsAsync_FailedDeletion_ReturnsFalse()
        {
            // Arrange
            var key = "test-key";
            var token = "valid-token";
            var cancellationToken = new CancellationToken();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri == new Uri(deleteStatisticsUrl + key) &&
                        req.Headers.Authorization.Parameter == token),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });
            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Act
            var result = await slotStatisticsService.DeleteSlotStatisticsAsync(key, token, cancellationToken);
            // Assert
            Assert.IsFalse(result);
        }
        [Test]
        public void DeleteSlotStatisticsAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            var key = "test-key";
            var token = "valid-token";
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Act & Assert
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await slotStatisticsService.DeleteSlotStatisticsAsync(key, token, cancellationTokenSource.Token));
        }
    }
}