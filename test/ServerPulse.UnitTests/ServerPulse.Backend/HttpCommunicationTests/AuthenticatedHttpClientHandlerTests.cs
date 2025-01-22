using HttpCommunication.DelegatingHandlers;
using Moq;
using Moq.Protected;

namespace HttpCommunicationTests
{
    [TestFixture]
    internal class AuthenticatedHttpClientHandlerTests
    {
        private Mock<DelegatingHandler> mockInnerHandler;
        private AuthenticatedHttpClientHandler handler;

        [SetUp]
        public void Setup()
        {
            mockInnerHandler = new Mock<DelegatingHandler> { CallBase = true };

            handler = new AuthenticatedHttpClientHandler
            {
                InnerHandler = mockInnerHandler.Object
            };
        }

        [TearDown]
        public void TearDown()
        {
            handler.Dispose();
        }

        [Test]
        public async Task SendAsync_ShouldConvertAuthorizationHeaderToBearer_WhenNonBearerTokenIsProvided()
        {
            // Arrange
            var token = "SampleToken";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            requestMessage.Headers.Add("Authorization", token);

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            mockInnerHandler.Protected()
                 .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                 .Returns(Task.FromResult(mockResponse))
                 .Verifiable();

            var invoker = new HttpMessageInvoker(handler);

            // Act
            var response = await invoker.SendAsync(requestMessage, CancellationToken.None);

            // Assert
            Assert.That(requestMessage.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
            Assert.That(requestMessage.Headers.Authorization.Parameter, Is.EqualTo(token));
            Assert.That(response, Is.EqualTo(mockResponse));
        }

        [Test]
        public async Task SendAsync_ShouldNotModifyAuthorizationHeader_WhenBearerTokenIsProvided()
        {
            // Arrange
            var token = "SampleToken";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            requestMessage.Headers.Add("Authorization", "Bearer " + token);

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            mockInnerHandler.Protected()
                 .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                 .Returns(Task.FromResult(mockResponse))
                 .Verifiable();

            var invoker = new HttpMessageInvoker(handler);

            // Act
            var response = await invoker.SendAsync(requestMessage, CancellationToken.None);

            // Assert
            Assert.That(requestMessage.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
            Assert.That(requestMessage.Headers.Authorization.Parameter, Is.EqualTo(token));
            Assert.That(response, Is.EqualTo(mockResponse));
        }

        [Test]
        public async Task SendAsync_ShouldNotAddAuthorizationHeader_WhenHeaderIsMissing()
        {
            // Arrange
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            mockInnerHandler.Protected()
                 .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                 .Returns(Task.FromResult(mockResponse))
                 .Verifiable();

            var invoker = new HttpMessageInvoker(handler);

            // Act
            var response = await invoker.SendAsync(requestMessage, CancellationToken.None);

            // Assert
            Assert.IsNull(requestMessage.Headers.Authorization);
            Assert.That(response, Is.EqualTo(mockResponse));
        }
    }
}
