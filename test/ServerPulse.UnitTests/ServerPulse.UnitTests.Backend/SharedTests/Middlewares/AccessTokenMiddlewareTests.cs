using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Middlewares;
using System.Security.Claims;

namespace SharedTests.Middlewares
{
    [TestFixture]
    internal class AccessTokenMiddlewareTests
    {
        private Mock<RequestDelegate> mockNext;
        private Mock<IAuthenticationService> mockAuthService;
        private Mock<IServiceProvider> mockServiceProvider;
        private AccessTokenMiddleware middleware;

        [SetUp]
        public void Setup()
        {
            mockNext = new Mock<RequestDelegate>();
            mockAuthService = new Mock<IAuthenticationService>();
            mockServiceProvider = new Mock<IServiceProvider>();
            middleware = new AccessTokenMiddleware(mockNext.Object);
        }

        [Test]
        public async Task Invoke_ShouldSetAccessTokenInHttpContextItems()
        {
            // Arrange
            var expectedToken = "test_access_token";
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), "TestScheme"));
            authResult.Properties.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = expectedToken } });
            mockAuthService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>())).ReturnsAsync(authResult);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService))).Returns(mockAuthService.Object);
            var httpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object };
            httpContext.Items = new System.Collections.Generic.Dictionary<object, object?>();
            // Act
            await middleware.Invoke(httpContext);
            // Assert
            Assert.That(httpContext.Items["AccessToken"], Is.EqualTo(expectedToken));
            mockNext.Verify(n => n(httpContext), Times.Once);
        }
        [Test]
        public async Task Invoke_ShouldProceedWithoutToken()
        {
            // Arrange
            var authResult = AuthenticateResult.NoResult();
            mockAuthService.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>())).ReturnsAsync(authResult);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService))).Returns(mockAuthService.Object);
            var httpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object };
            httpContext.Items = new System.Collections.Generic.Dictionary<object, object?>();
            // Act
            await middleware.Invoke(httpContext);
            // Assert
            Assert.IsNull(httpContext.Items["AccessToken"]);
            mockNext.Verify(n => n(httpContext), Times.Once);
        }
    }
}