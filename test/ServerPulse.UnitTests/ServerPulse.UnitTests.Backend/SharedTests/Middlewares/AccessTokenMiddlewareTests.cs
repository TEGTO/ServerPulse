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

        private static IEnumerable<TestCaseData> GetAccessTokenTestCases()
        {
            var authSuccesfulResult = AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), "TestScheme"));
            authSuccesfulResult.Properties!.StoreTokens([new AuthenticationToken { Name = "access_token", Value = "test_access_token" }]);

            yield return new TestCaseData(
                authSuccesfulResult,
                "test_access_token"
            ).SetDescription("Token exists and should be set in the HttpContext.Items");

            yield return new TestCaseData(
                AuthenticateResult.NoResult(),
                null
            ).SetDescription("Authentication returns no result; no token should be set in HttpContext.Items");

            yield return new TestCaseData(
                AuthenticateResult.Fail("Authentication failed"),
                null
            ).SetDescription("Authentication fails; no token should be set in HttpContext.Items");

            yield return new TestCaseData(
                null,
                null
            ).SetDescription("Authentication result is null; no token should be set in HttpContext.Items");
        }

        [TestCaseSource(nameof(GetAccessTokenTestCases))]
        public async Task Invoke_ShouldHandleAccessTokenCorrectly(
          AuthenticateResult authResult,
          string expectedToken)
        {
            // Arrange
            mockAuthService
                .Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(authResult);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);

            var httpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object };
            httpContext.Items = new Dictionary<object, object?>();

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
            httpContext.Items = new Dictionary<object, object?>();

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            Assert.IsNull(httpContext.Items["AccessToken"]);

            mockNext.Verify(n => n(httpContext), Times.Once);
        }
    }
}