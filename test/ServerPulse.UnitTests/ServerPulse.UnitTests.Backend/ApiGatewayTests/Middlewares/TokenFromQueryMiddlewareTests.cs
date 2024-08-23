using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace ApiGateway.Middlewares.Tests
{
    [TestFixture]
    internal class TokenFromQueryMiddlewareTests
    {
        private Mock<RequestDelegate> mockNext;
        private TokenFromQueryMiddleware middleware;

        [SetUp]
        public void Setup()
        {
            mockNext = new Mock<RequestDelegate>();
            middleware = new TokenFromQueryMiddleware(mockNext.Object);
        }

        [Test]
        public async Task Invoke_WithHubPathAndAccessToken_AddsAuthorizationHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/hub/somepath";
            context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "access_token", "test-token" }
            });
            // Act
            await middleware.Invoke(context);
            // Assert
            Assert.IsTrue(context.Request.Headers.ContainsKey("Authorization"));
            Assert.That(context.Request.Headers["Authorization"], Is.EqualTo("Bearer test-token"));
            mockNext.Verify(next => next(context), Times.Once);
        }
        [Test]
        public async Task Invoke_WithoutHubPath_DoesNotAddAuthorizationHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/someotherpath";
            context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "access_token", "test-token" }
            });
            // Act
            await middleware.Invoke(context);
            // Assert
            Assert.IsFalse(context.Request.Headers.ContainsKey("Authorization"));
            mockNext.Verify(next => next(context), Times.Once);
        }
        [Test]
        public async Task Invoke_WithHubPathWithoutAccessToken_DoesNotAddAuthorizationHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/hub/somepath";
            // Act
            await middleware.Invoke(context);
            // Assert
            Assert.IsFalse(context.Request.Headers.ContainsKey("Authorization"));
            mockNext.Verify(next => next(context), Times.Once);
        }
        [Test]
        public async Task Invoke_AlwaysCallsNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/hub/somepath";
            context.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "access_token", "test-token" }
            });
            // Act
            await middleware.Invoke(context);
            // Assert
            mockNext.Verify(next => next(context), Times.Once);
        }
    }
}