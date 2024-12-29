using Moq;
using Moq.Protected;
using ServerPulse.Client.Services;
using System.Net;

namespace ServerPulse.ClientTests.Services.Tests
{
    [TestFixture]
    internal class MessageSenderTests
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
            var httpClient = new HttpClient(handlerMock.Object);

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri(url) &&
                        req.Content!.ReadAsStringAsync()!.Result == json &&
                        req.Content.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

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
            var httpClient = new HttpClient(handlerMock.Object);

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri(url) &&
                        req.Content!.ReadAsStringAsync()!.Result == json &&
                        req.Content.Headers.ContentType!.MediaType == "application/json"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await messageSender.SendJsonAsync(json, url, cancellationToken);
            });
        }

        [Test]
        public void SendJsonAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";
            var url = "https://example.com/api";
            var cancellationTokenSource = new CancellationTokenSource();
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);

            cancellationTokenSource.Cancel();

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

            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act & Assert
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await messageSender.SendJsonAsync(json, url, cancellationTokenSource.Token);
            });

            cancellationTokenSource.Dispose();
        }
    }
}
