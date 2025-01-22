using Microsoft.Extensions.Options;
using Moq;

namespace Authentication.OAuth.Google.Tests
{
    [TestFixture]
    internal class GoogleOAuthClientTests
    {
        private Mock<IGoogleOAuthApi> mockGoogleOAuthApi;
        private GoogleOAuthSettings mockOAuthSettings;
        private GoogleOAuthClient googleOAuthHttpClient;

        [SetUp]
        public void SetUp()
        {
            mockOAuthSettings = new GoogleOAuthSettings
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                Scope = ""
            };

            mockGoogleOAuthApi = new Mock<IGoogleOAuthApi>();

            var mockOptions = new Mock<IOptions<GoogleOAuthSettings>>();
            mockOptions.Setup(x => x.Value).Returns(mockOAuthSettings);

            googleOAuthHttpClient = new GoogleOAuthClient(mockOptions.Object, mockGoogleOAuthApi.Object);
        }

        [Test]
        public async Task ExchangeAuthorizationCodeAsync_ValidParams_ReturnsTokenResult()
        {
            // Arrange
            var code = "valid-auth-code";
            var codeVerifier = "valid-code-verifier";
            var redirectUrl = "https://example.com/callback";

            var expectedTokenResult = new GoogleOAuthTokenResult
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            mockGoogleOAuthApi.Setup(x => x.ExchangeAuthorizationCodeAsync(
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(expectedTokenResult);

            // Act
            var result = await googleOAuthHttpClient.ExchangeAuthorizationCodeAsync(code, codeVerifier, redirectUrl, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.AccessToken, Is.EqualTo(expectedTokenResult.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(expectedTokenResult.RefreshToken));
        }

        [Test]
        public void GenerateOAuthRequestUrl_ValidParams_ReturnsCorrectUrl()
        {
            // Arrange
            var redirectUrl = "https://example.com/callback";
            var codeVerifier = "valid-code-verifier";
            var scope = "https://www.googleapis.com/auth/userinfo.email";

            // Act
            var resultUrl = googleOAuthHttpClient.GenerateOAuthRequestUrl(redirectUrl, codeVerifier, scope);

            // Assert
            Assert.IsTrue(resultUrl.Contains("client_id=test-client-id"));
            Assert.IsTrue(resultUrl.Contains("redirect_uri=https%3A%2F%2Fexample.com%2Fcallback"));
            Assert.IsTrue(resultUrl.Contains("response_type=code"));
            Assert.IsTrue(resultUrl.Contains("scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email"));
            Assert.IsTrue(resultUrl.Contains("code_challenge="));
            Assert.IsTrue(resultUrl.Contains("code_challenge_method=S256"));
            Assert.IsTrue(resultUrl.Contains("access_type=offline"));
        }
    }
}