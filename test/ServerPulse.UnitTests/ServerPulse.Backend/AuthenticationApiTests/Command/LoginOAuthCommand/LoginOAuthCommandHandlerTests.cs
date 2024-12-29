using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Moq;

namespace AuthenticationApi.Command.LoginOAuthCommand.Tests
{
    [TestFixture]
    internal class LoginOAuthCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private Mock<IOAuthService> mockOAuthService;
        private Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;
        private LoginOAuthCommandHandler handler;

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

            handler = new LoginOAuthCommandHandler(oAuthServices, mockAuthService.Object, mockMapper.Object);
        }

        [Test]
        public async Task Handle_ValidRequest_ReturnsUserAuthenticationResponse()
        {
            // Arrange
            var request = new LoginOAuthCommand(new UserOAuthenticationRequest
            {
                Code = "test-code",
                CodeVerifier = "test-code-verifier",
                RedirectUrl = "https://example.com/callback",
                OAuthLoginProvider = OAuthLoginProvider.Google
            });

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
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Email, Is.EqualTo("user@example.com"));
            Assert.That(result.AuthToken, Is.EqualTo(accessTokenDataDto));
        }

        [Test]
        public void Handle_InvalidOAuthProvider_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new LoginOAuthCommand(new UserOAuthenticationRequest
            {
                Code = "test-code",
                CodeVerifier = "test-code-verifier",
                RedirectUrl = "https://example.com/callback",
                OAuthLoginProvider = (OAuthLoginProvider)999
            });

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await handler.Handle(request, CancellationToken.None);
            });
        }

        [Test]
        public void Handle_NullLoginModel_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new LoginOAuthCommand(new UserOAuthenticationRequest
            {
                Code = "test-code",
                CodeVerifier = "test-code-verifier",
                RedirectUrl = "https://example.com/callback",
                OAuthLoginProvider = OAuthLoginProvider.Google
            });

            mockOAuthService.Setup(x => x.GetProviderModelOnCodeAsync(It.IsAny<OAuthAccessCodeParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProviderLoginModel?)null!);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await handler.Handle(request, CancellationToken.None);
            });
        }
    }
}