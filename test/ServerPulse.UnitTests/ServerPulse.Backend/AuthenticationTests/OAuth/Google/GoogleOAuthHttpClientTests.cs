using Helper.Services;
using Moq;

namespace Authentication.OAuth.Google.Tests
{
    [TestFixture]
    internal class GoogleOAuthHttpClientTests
    {
        private Mock<IHttpHelper> mockHttpHelper;
        private GoogleOAuthSettings mockOAuthSettings;
        private GoogleOAuthHttpClient googleOAuthHttpClient;

        [SetUp]
        public void SetUp()
        {
            mockHttpHelper = new Mock<IHttpHelper>();
            mockOAuthSettings = new GoogleOAuthSettings
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                Scope = ""
            };

            googleOAuthHttpClient = new GoogleOAuthHttpClient(mockOAuthSettings, mockHttpHelper.Object);
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

            mockHttpHelper.Setup(x => x.SendPostRequestAsync<GoogleOAuthTokenResult>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                 It.IsAny<string>(),
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
            var scope = "https://www.googleapis.com/auth/userinfo.email";
            var redirectUrl = "https://example.com/callback";
            var codeVerifier = "valid-code-verifier";
            var expectedBaseUrl = "https://accounts.google.com/o/oauth2/v2/auth";

            // Act
            var resultUrl = googleOAuthHttpClient.GenerateOAuthRequestUrl(scope, redirectUrl, codeVerifier);

            // Assert
            Assert.IsTrue(resultUrl.StartsWith(expectedBaseUrl));
            Assert.IsTrue(resultUrl.Contains("client_id=test-client-id"));
            Assert.IsTrue(resultUrl.Contains("redirect_uri=https%3A%2F%2Fexample.com%2Fcallback"));
            Assert.IsTrue(resultUrl.Contains("response_type=code"));
            Assert.IsTrue(resultUrl.Contains("scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email"));
            Assert.IsTrue(resultUrl.Contains("code_challenge="));
            Assert.IsTrue(resultUrl.Contains("code_challenge_method=S256"));
            Assert.IsTrue(resultUrl.Contains("access_type=offline"));
        }

        [Test]
        public async Task RefreshAccessTokenAsync_ValidParams_ReturnsNewTokenResult()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var cancellationToken = CancellationToken.None;

            var expectedTokenResult = new GoogleOAuthTokenResult
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            mockHttpHelper.Setup(x => x.SendPostRequestAsync<GoogleOAuthTokenResult>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTokenResult);

            // Act
            var result = await googleOAuthHttpClient.RefreshAccessTokenAsync(refreshToken, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.AccessToken, Is.EqualTo(expectedTokenResult.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(expectedTokenResult.RefreshToken));
        }
    }
}