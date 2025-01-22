﻿using Authentication.OAuth.GitHub;
using AuthenticationApi.Core.Enums;
using Moq;

namespace AuthenticationApi.Application.Services.Tests
{
    [TestFixture]
    internal class GitHubOAuthServiceTests
    {
        private Mock<IGitHubOAuthClient> oauthClientMock;
        private Mock<IGitHubApi> apiClientMock;
        private Mock<IStringVerifierService> stringVerifierMock;
        private GitHubOAuthService gitHubOAuthService;

        [SetUp]
        public void SetUp()
        {
            oauthClientMock = new Mock<IGitHubOAuthClient>();
            apiClientMock = new Mock<IGitHubApi>();
            stringVerifierMock = new Mock<IStringVerifierService>();

            gitHubOAuthService = new GitHubOAuthService(
                oauthClientMock.Object,
                apiClientMock.Object,
                stringVerifierMock.Object
            );
        }

        [Test]
        public async Task GetProviderModelOnCodeAsync_ValidCode_ReturnsProviderLoginModel()
        {
            // Arrange
            var code = "test-code";
            var redirectUrl = "http://test-redirect.com";
            var tokenResult = new GitHubOAuthTokenResult
            {
                AccessToken = "valid-access-token"
            };
            var userResult = new GitHubUserResult
            {
                Id = 12345,
                Email = "user@example.com"
            };

            oauthClientMock
                .Setup(x => x.ExchangeAuthorizationCodeAsync(code, redirectUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenResult);

            apiClientMock
                .Setup(x => x.GetUserInfoAsync(tokenResult.AccessToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResult);

            // Act
            var result = await gitHubOAuthService.GetProviderModelOnCodeAsync(code, redirectUrl, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("user@example.com"));
            Assert.That(result.ProviderLogin, Is.EqualTo(nameof(OAuthLoginProvider.Google)));
            Assert.That(result.ProviderKey, Is.EqualTo(userResult.Id.ToString()));

            oauthClientMock.Verify(x => x.ExchangeAuthorizationCodeAsync(code, redirectUrl, It.IsAny<CancellationToken>()), Times.Once);
            apiClientMock.Verify(x => x.GetUserInfoAsync("valid-access-token", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetProviderModelOnCodeAsync_InvalidToken_ThrowsException()
        {
            // Arrange
            var code = "test-code";
            var redirectUrl = "http://test-redirect.com";

            oauthClientMock
                .Setup(x => x.ExchangeAuthorizationCodeAsync(code, redirectUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GitHubOAuthTokenResult?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await gitHubOAuthService.GetProviderModelOnCodeAsync(code, redirectUrl, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo("Can't get the user access token!"));
            oauthClientMock.Verify(x => x.ExchangeAuthorizationCodeAsync(code, redirectUrl, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GenerateOAuthRequestUrlAsync_ValidParams_ReturnsUrl()
        {
            // Arrange
            var redirectUrl = "http://test-redirect.com";
            var stateVerifier = "state-verifier";
            var expectedUrl = "http://generated-url.com";

            stringVerifierMock
                .Setup(x => x.GetStringVerifierAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stateVerifier);

            oauthClientMock
            .Setup(x => x.GenerateOAuthRequestUrl(redirectUrl, stateVerifier, It.IsAny<string>()))
            .Returns(expectedUrl);

            // Act
            var result = await gitHubOAuthService.GenerateOAuthRequestUrlAsync(redirectUrl, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(expectedUrl));
            stringVerifierMock.Verify(x => x.GetStringVerifierAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetProviderModelOnCodeAsync_UserInfoNull_ThrowsException()
        {
            // Arrange
            var code = "test-code";
            var redirectUrl = "http://test-redirect.com";
            var tokenResult = new GitHubOAuthTokenResult
            {
                AccessToken = "valid-access-token"
            };

            oauthClientMock
                .Setup(x => x.ExchangeAuthorizationCodeAsync(code, redirectUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenResult);

            apiClientMock
                .Setup(x => x.GetUserInfoAsync(tokenResult.AccessToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GitHubUserResult?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await gitHubOAuthService.GetProviderModelOnCodeAsync(code, redirectUrl, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo("Can't get the user!"));
            oauthClientMock.Verify(x => x.ExchangeAuthorizationCodeAsync(code, redirectUrl, It.IsAny<CancellationToken>()), Times.Once);
            apiClientMock.Verify(x => x.GetUserInfoAsync("valid-access-token", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}