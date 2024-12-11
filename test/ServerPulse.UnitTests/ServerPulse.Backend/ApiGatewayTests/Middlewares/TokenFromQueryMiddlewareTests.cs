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
        [TestCase("/hub/somepath", "test-token", true, "Bearer test-token")]
        [TestCase("/HUB/somepath", "test-token", true, "Bearer test-token")]
        [TestCase(null, "test-token", false, "Bearer test-token")]
        [TestCase("/hub", "another-token", true, "Bearer another-token")]
        [TestCase("/someotherpath", "test-token", false, null, Description = "Path without \"hub\"")]
        [TestCase("/hub/somepath", null, false, null, Description = "Missing access token")]
        [TestCase("/hub/somepath", null, false, null, Description = "Empty access token")]
        public async Task Invoke_VariousScenarios_VerifiesAuthorizationHeader(string? path, string? accessToken, bool shouldAddHeader, string? expectedHeaderValue)
        {
            // Arrange
            var queryDict = new Dictionary<string, StringValues>();
            if (accessToken != null)
            {
                queryDict.Add("access_token", accessToken);
            }

            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Query = new QueryCollection(queryDict);

            // Act
            await middleware.Invoke(context);

            // Assert
            if (shouldAddHeader)
            {
                Assert.IsTrue(context.Request.Headers.ContainsKey("Authorization"));
                Assert.That(context.Request.Headers["Authorization"], Is.EqualTo(expectedHeaderValue));
            }
            else
            {
                Assert.IsFalse(context.Request.Headers.ContainsKey("Authorization"));
            }

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