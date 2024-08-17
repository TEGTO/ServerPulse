using Moq;
using Moq.Protected;
using ServerPulse.Client.Services;
using System.Net;

namespace ServerPulse.ClientTests.Services
{
    [TestFixture]
    public class MessageSenderTests
    {
        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private MessageSender messageSender;

        [SetUp]
        public void SetUp()
        {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            messageSender = new MessageSender(httpClientFactoryMock.Object);
        }

        [Test]
        public async Task SendJsonAsync_SuccessfulPost_ReturnsSuccess()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";
            var url = "https://example.com/api";
            var cancellationToken = new CancellationToken();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri(url) &&
                        req.Content.ReadAsStringAsync().Result == json &&
                        req.Content.Headers.ContentType.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            await messageSender.SendJsonAsync(json, url, cancellationToken);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri(url)),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        [Test]
        public void SendJsonAsync_FailedPost_ThrowsHttpRequestException()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";
            var url = "https://example.com/api";
            var cancellationToken = new CancellationToken();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri(url) &&
                        req.Content.ReadAsStringAsync().Result == json &&
                        req.Content.Headers.ContentType.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await messageSender.SendJsonAsync(json, url, cancellationToken));
        }
        [Test]
        public void SendJsonAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";
            var url = "https://example.com/api";
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
                await messageSender.SendJsonAsync(json, url, cancellationTokenSource.Token));
        }
    }
}
