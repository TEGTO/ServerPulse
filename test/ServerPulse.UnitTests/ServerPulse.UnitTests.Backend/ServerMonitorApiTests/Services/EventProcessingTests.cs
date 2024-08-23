using Microsoft.Extensions.Configuration;
using Moq;
using ServerPulse.EventCommunication.Events;
using System.Net;
using System.Text.Json;

namespace ServerMonitorApi.Services.Tests
{
    [TestFixture]
    internal class EventProcessingTests
    {
        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<MockHttpMessageHandler> mockHttpMessageHandler;
        private EventProcessing eventProcessing;
        private const string ApiGatewayUrl = "http://api.gateway/";
        private const string LoadAnalyzeUri = "analyze/load";

        [SetUp]
        public void Setup()
        {
            mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockConfiguration = new Mock<IConfiguration>();
            mockHttpMessageHandler = new Mock<MockHttpMessageHandler> { CallBase = true };

            mockConfiguration.Setup(c => c[Configuration.API_GATEWAY]).Returns(ApiGatewayUrl);
            mockConfiguration.Setup(c => c[Configuration.ANALYZER_LOAD_ANALYZE]).Returns(LoadAnalyzeUri);

            eventProcessing = new EventProcessing(mockHttpClientFactory.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task SendEventsForProcessingsAsync_ShouldSendPostRequest_WhenEventsAreProvided()
        {
            // Arrange
            var events = new[] { new LoadEvent("test-key", "endpoint", "method", 200, TimeSpan.Zero, DateTime.MinValue) };
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            mockHttpMessageHandler
                .Setup(handler => handler.SendAsyncMock(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
            // Act
            await eventProcessing.SendEventsForProcessingsAsync(events, CancellationToken.None);
            // Assert
            var s = JsonSerializer.Serialize(events);
            mockHttpMessageHandler.Verify(
                handler => handler.SendAsyncMock(It.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri($"{ApiGatewayUrl}{LoadAnalyzeUri}") &&
                    req.Content.ReadAsStringAsync().Result == s),
                    It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        [Test]
        public async Task SendEventsForProcessingsAsync_ShouldNotSendRequest_WhenEventsArrayIsEmpty()
        {
            // Arrange
            var events = new BaseEvent[0];
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Act
            await eventProcessing.SendEventsForProcessingsAsync(events, CancellationToken.None);
            // Assert
            mockHttpMessageHandler.Verify(
                handler => handler.SendAsyncMock(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
        [Test]
        public async Task SendEventsForProcessingsAsync_ShouldNotSendRequest_WhenEventsIsNull()
        {
            // Arrange
            BaseEvent[]? events = null;
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            // Act
            await eventProcessing.SendEventsForProcessingsAsync(events, CancellationToken.None);
            // Assert
            mockHttpMessageHandler.Verify(
                handler => handler.SendAsyncMock(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public virtual Task<HttpResponseMessage> SendAsyncMock(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsyncMock(request, cancellationToken);
        }
    }
}