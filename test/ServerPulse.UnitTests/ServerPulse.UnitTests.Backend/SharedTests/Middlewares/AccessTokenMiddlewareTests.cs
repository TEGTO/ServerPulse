using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Middlewares;

namespace SharedTests.Middlewares
{
    [TestFixture]
    internal class AccessTokenMiddlewareTests
    {
        private Mock<RequestDelegate> mockNext;
        private AccessTokenMiddleware middleware;

        [SetUp]
        public void SetUp()
        {
            mockNext = new Mock<RequestDelegate>();
            middleware = new AccessTokenMiddleware(mockNext.Object);
        }

        //[Test]
        //public async Task Invoke_ShouldContinuePipelineEvenIfAccessTokenIsNull()
        //{
        //    // Arrange
        //    var httpContext = new DefaultHttpContext();

        //    // Act
        //    await middleware.Invoke(httpContext);

        //    // Assert
        //    Assert.IsTrue(httpContext.Items.ContainsKey("AccessToken"));
        //    Assert.IsNull(httpContext.Items["AccessToken"]);

        //    mockNext.Verify(next => next(httpContext), Times.Once);
        //}
    }
}