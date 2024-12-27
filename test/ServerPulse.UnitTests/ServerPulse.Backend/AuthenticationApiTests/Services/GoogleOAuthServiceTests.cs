using Authentication.OAuth.Google;
using AuthenticationApi.Dtos.OAuth;
using Microsoft.Extensions.Options;
using Moq;
using System.ComponentModel.DataAnnotations;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace AuthenticationApi.Services.Tests
{
    [TestFixture]
    internal class GoogleOAuthServiceTests
    {
        private Mock<IGoogleOAuthHttpClient> httpClientMock;
        private Mock<IGoogleTokenValidator> googleTokenValidatorMock;
        private GoogleOAuthSettings googleOAuthSettings;
        private GoogleOAuthService googleOAuthService;

        [SetUp]
        public void SetUp()
        {
            googleOAuthSettings = new GoogleOAuthSettings
            {
                ClientId = "test-client-id",
                ClientSecret = "some-secret",
                Scope = "test-scope"
            };

            httpClientMock = new Mock<IGoogleOAuthHttpClient>();
            googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();
            var optionsMock = new Mock<IOptions<GoogleOAuthSettings>>();
            optionsMock.Setup(x => x.Value).Returns(googleOAuthSettings);

            googleOAuthService = new GoogleOAuthService(httpClientMock.Object, googleTokenValidatorMock.Object, optionsMock.Object);
        }

        [Test]
        public async Task GetProviderModelOnCodeAsync_ValidCode_ReturnsProviderLoginModel()
        {
            // Arrange
            var tokenResult = new GoogleOAuthTokenResult
            {
                IdToken = "test-id-token"
            };

            var payload = new Payload
            {
                Email = "user@example.com",
                Subject = "subject-id"
            };

            var requestParams = new OAuthAccessCodeParams("test-code", "test-code-verifier", "http://test-redirect.com");

            httpClientMock
                .Setup(x => x.ExchangeAuthorizationCodeAsync("test-code", "test-code-verifier", "http://test-redirect.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenResult);

            googleTokenValidatorMock
                .Setup(x => x.ValidateAsync("test-id-token", It.IsAny<ValidationSettings>()))
                .ReturnsAsync(payload);

            // Act
            var result = await googleOAuthService.GetProviderModelOnCodeAsync(requestParams, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("user@example.com"));
            Assert.That(result.ProviderLogin, Is.EqualTo(nameof(OAuthLoginProvider.Google)));
            Assert.That(result.ProviderKey, Is.EqualTo("subject-id"));

            httpClientMock.Verify(x => x.ExchangeAuthorizationCodeAsync("test-code", "test-code-verifier", "http://test-redirect.com", It.IsAny<CancellationToken>()), Times.Once);
            googleTokenValidatorMock.Verify(x => x.ValidateAsync("test-id-token", It.IsAny<ValidationSettings>()), Times.Once);
        }

        [Test]
        public void GetProviderModelOnCodeAsync_InvalidToken_ThrowsException()
        {
            // Arrange
            var tokenResult = new GoogleOAuthTokenResult
            {
                IdToken = "invalid-id-token"
            };

            var requestParams = new OAuthAccessCodeParams("test-code", "test-code-verifier", "http://test-redirect.com");

            httpClientMock
                .Setup(x => x.ExchangeAuthorizationCodeAsync("test-code", "test-code-verifier", "http://test-redirect.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenResult);

            googleTokenValidatorMock
                .Setup(x => x.ValidateAsync("invalid-id-token", It.IsAny<ValidationSettings>()))
                .ThrowsAsync(new ValidationException("Invalid token"));

            // Act & Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await googleOAuthService.GetProviderModelOnCodeAsync(requestParams, CancellationToken.None));

            httpClientMock.Verify(x => x.ExchangeAuthorizationCodeAsync("test-code", "test-code-verifier", "http://test-redirect.com", It.IsAny<CancellationToken>()), Times.Once);
            googleTokenValidatorMock.Verify(x => x.ValidateAsync("invalid-id-token", It.IsAny<ValidationSettings>()), Times.Once);
        }

        [Test]
        public void GenerateOAuthRequestUrl_ValidParams_ReturnsUrl()
        {
            // Arrange
            var requestParams = new OAuthRequestUrlParams("http://test-redirect.com", "test-code-verifier");

            httpClientMock
                .Setup(x => x.GenerateOAuthRequestUrl("test-scope", "http://test-redirect.com", "test-code-verifier"))
                .Returns("http://generated-url.com");

            // Act
            var result = googleOAuthService.GenerateOAuthRequestUrl(requestParams);

            // Assert
            Assert.That(result, Is.EqualTo("http://generated-url.com"));
            httpClientMock.Verify(x => x.GenerateOAuthRequestUrl("test-scope", "http://test-redirect.com", "test-code-verifier"), Times.Once);
        }
    }
}