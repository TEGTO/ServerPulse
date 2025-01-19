using Helper.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Authentication.OAuth.GitHub.Tests
{
    [TestFixture]
    internal class GitHubOAuthClientTests
    {
        private Mock<IHttpHelper> mockHttpHelper;
        private GitHubOAuthSettings mockOAuthSettings;
        private GitHubOAuthClient gitHubOAuthClient;

        [SetUp]
        public void SetUp()
        {
            mockOAuthSettings = new GitHubOAuthSettings
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                AppName = "TestApp"
            };

            mockHttpHelper = new Mock<IHttpHelper>();

            var mockOptions = new Mock<IOptions<GitHubOAuthSettings>>();
            mockOptions.Setup(x => x.Value).Returns(mockOAuthSettings);

            gitHubOAuthClient = new GitHubOAuthClient(mockOptions.Object, mockHttpHelper.Object);
        }

        [Test]
        public async Task ExchangeAuthorizationCodeAsync_ValidParams_ReturnsTokenResult()
        {
            // Arrange
            var code = "valid-auth-code";
            var redirectUrl = "https://example.com/callback";

            var expectedTokenResult = new GitHubOAuthTokenResult
            {
                AccessToken = "access-token",
                TokenType = "bearer",
                Scope = "repo,user"
            };

            mockHttpHelper.Setup(x => x.SendPostRequestAsync<GitHubOAuthTokenResult>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(expectedTokenResult);

            // Act
            var result = await gitHubOAuthClient.ExchangeAuthorizationCodeAsync(code, redirectUrl, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.AccessToken, Is.EqualTo(expectedTokenResult.AccessToken));
            Assert.That(result.TokenType, Is.EqualTo(expectedTokenResult.TokenType));
            Assert.That(result.Scope, Is.EqualTo(expectedTokenResult.Scope));

            mockHttpHelper.Verify(x => x.SendPostRequestAsync<GitHubOAuthTokenResult>(
                "https://github.com/login/oauth/access_token",
                It.Is<Dictionary<string, string>>(dict =>
                    dict["client_id"] == mockOAuthSettings.ClientId &&
                    dict["client_secret"] == mockOAuthSettings.ClientSecret &&
                    dict["code"] == code &&
                    dict["redirect_uri"] == redirectUrl),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GenerateOAuthRequestUrl_ValidParams_ReturnsCorrectUrl()
        {
            // Arrange
            var scope = "repo,user";
            var redirectUrl = "https://example.com/callback";
            var stateVerifier = "valid-state-verifier";

            var expectedBaseUrl = "https://github.com/login/oauth/authorize";

            // Act
            var resultUrl = gitHubOAuthClient.GenerateOAuthRequestUrl(scope, redirectUrl, stateVerifier);

            // Assert
            Assert.IsTrue(resultUrl.StartsWith(expectedBaseUrl));
            Assert.IsTrue(resultUrl.Contains("client_id=test-client-id"));
            Assert.IsTrue(resultUrl.Contains("redirect_uri=https%3A%2F%2Fexample.com%2Fcallback"));
            Assert.IsTrue(resultUrl.Contains("scope=repo,user"));
            Assert.IsTrue(resultUrl.Contains("state=" + HashHelper.ComputeHash(stateVerifier)));
        }
    }
}