using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthenticationApi.Endpoints.OAuth.LoginOAuth.Tests
{
    [TestFixture]
    internal class LoginOAuthControllerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private Mock<IOAuthService> mockOAuthService;
        private Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;
        private LoginOAuthController controller;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();
            mockOAuthService = new Mock<IOAuthService>();

            oAuthServices = new Dictionary<OAuthLoginProvider, IOAuthService>
            {
                { OAuthLoginProvider.Google, mockOAuthService.Object }
            };

            controller = new LoginOAuthController(oAuthServices, mockAuthService.Object, mockMapper.Object);
        }

        [Test]
        public async Task LoginOAuth_ValidRequest_ReturnsOkResponse()
        {
            // Arrange
            var request = new LoginOAuthRequest
            {
                Code = "test-code",
                CodeVerifier = "test-code-verifier",
                RedirectUrl = "https://example.com/callback",
                OAuthLoginProvider = OAuthLoginProvider.Google
            };

            var providerLoginModel = new ProviderLoginModel
            {
                Email = "user@example.com",
                ProviderLogin = nameof(OAuthLoginProvider.Google),
                ProviderKey = "google-key"
            };

            var accessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            var accessTokenDataDto = new AccessTokenDataDto
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            mockOAuthService.Setup(x => x.GetProviderModelOnCodeAsync(It.IsAny<OAuthAccessCodeParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(providerLoginModel);

            mockAuthService.Setup(x => x.LoginUserWithProviderAsync(providerLoginModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accessTokenData);

            mockMapper.Setup(x => x.Map<AccessTokenDataDto>(accessTokenData))
                .Returns(accessTokenDataDto);

            // Act
            var result = await controller.LoginOAuth(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var response = (result.Result as OkObjectResult)?.Value as LoginOAuthResponse;

            Assert.NotNull(response);
            Assert.That(response.Email, Is.EqualTo("user@example.com"));
            Assert.That(response.AccessTokenData, Is.EqualTo(accessTokenDataDto));
        }

        [Test]
        public void LoginOAuth_InvalidOAuthProvider_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new LoginOAuthRequest
            {
                Code = "test-code",
                CodeVerifier = "test-code-verifier",
                RedirectUrl = "https://example.com/callback",
                OAuthLoginProvider = (OAuthLoginProvider)999
            };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await controller.LoginOAuth(request, CancellationToken.None);
            });
        }

        [Test]
        public async Task LoginOAuth_NullLoginModel_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginOAuthRequest
            {
                Code = "test-code",
                CodeVerifier = "test-code-verifier",
                RedirectUrl = "https://example.com/callback",
                OAuthLoginProvider = OAuthLoginProvider.Google
            };

            mockOAuthService.Setup(x => x.GetProviderModelOnCodeAsync(It.IsAny<OAuthAccessCodeParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProviderLoginModel?)null!);

            // Act
            var result = await controller.LoginOAuth(request, CancellationToken.None);

            // Assert
            var response = result.Result as UnauthorizedObjectResult;

            Assert.IsNotNull(response, "Result should be UnauthorizedObjectResult.");
            Assert.That(response.Value, Is.EqualTo("Login model provider is null!"));
        }
    }
}