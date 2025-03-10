﻿using Authentication.OAuth.Google;
using AuthenticationApi.Core.Enums;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using System.ComponentModel.DataAnnotations;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace AuthenticationApi.Application.Services.Tests
{
    [TestFixture]
    internal class GoogleOAuthServiceTests
    {
        private Mock<IGoogleOAuthClient> googleClienttMock;
        private Mock<IGoogleTokenValidator> googleTokenValidatorMock;
        private Mock<IStringVerifierService> stringVerifierMock;
        private GoogleOAuthService googleOAuthService;

        [SetUp]
        public void SetUp()
        {
            googleClienttMock = new Mock<IGoogleOAuthClient>();
            googleTokenValidatorMock = new Mock<IGoogleTokenValidator>();
            stringVerifierMock = new Mock<IStringVerifierService>();

            googleOAuthService = new GoogleOAuthService(
                googleClienttMock.Object,
                googleTokenValidatorMock.Object,
                stringVerifierMock.Object
            );
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
            var code = "test-code";
            var redirectUrl = "http://test-redirect.com";
            var queryParams = QueryHelpers.AddQueryString("", new Dictionary<string, string?>
            {
                { "code", code }
            });

            googleClienttMock
                .Setup(x => x.ExchangeAuthorizationCodeAsync(code, It.IsAny<string>(), redirectUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenResult);

            googleTokenValidatorMock
                .Setup(x => x.ValidateAsync(tokenResult.IdToken))
                .ReturnsAsync(payload);

            // Act
            var result = await googleOAuthService.GetProviderModelOnCodeAsync(queryParams, redirectUrl, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("user@example.com"));
            Assert.That(result.ProviderLogin, Is.EqualTo(nameof(OAuthLoginProvider.Google)));
            Assert.That(result.ProviderKey, Is.EqualTo("subject-id"));

            googleClienttMock.Verify(x => x.ExchangeAuthorizationCodeAsync(code, It.IsAny<string>(), redirectUrl, It.IsAny<CancellationToken>()), Times.Once);
            googleTokenValidatorMock.Verify(x => x.ValidateAsync("test-id-token"), Times.Once);
            stringVerifierMock.Verify(x => x.GetStringVerifierAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetProviderModelOnCodeAsync_InvalidToken_ThrowsException()
        {
            // Arrange
            var tokenResult = new GoogleOAuthTokenResult
            {
                IdToken = "invalid-id-token"
            };
            var code = "test-code";
            var redirectUrl = "http://test-redirect.com";
            var queryParams = QueryHelpers.AddQueryString("", new Dictionary<string, string?>
            {
                { "code", code }
            });

            googleClienttMock
                .Setup(x => x.ExchangeAuthorizationCodeAsync(code, It.IsAny<string>(), redirectUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenResult);

            googleTokenValidatorMock
                .Setup(x => x.ValidateAsync(tokenResult.IdToken))
                .ThrowsAsync(new ValidationException("Invalid token"));

            // Act & Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await googleOAuthService.GetProviderModelOnCodeAsync(queryParams, redirectUrl, CancellationToken.None));

            googleClienttMock.Verify(x => x.ExchangeAuthorizationCodeAsync(code, It.IsAny<string>(), redirectUrl, It.IsAny<CancellationToken>()), Times.Once);
            googleTokenValidatorMock.Verify(x => x.ValidateAsync("invalid-id-token"), Times.Once);
            stringVerifierMock.Verify(x => x.GetStringVerifierAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetProviderModelOnCodeAsync_EmptyCode_ThrowsException()
        {
            // Arrange
            var code = "";
            var queryParams = QueryHelpers.AddQueryString("", new Dictionary<string, string?>
            {
                { "code", code }
            });
            var redirectUrl = "http://test-redirect.com";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                 await googleOAuthService.GetProviderModelOnCodeAsync(queryParams, redirectUrl, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo("Code is null or empty."));
        }

        [Test]
        public async Task GenerateOAuthRequestUrlAsync_ValidParams_ReturnsUrl()
        {
            // Arrange
            var redirectUrl = "http://test-redirect.com";

            googleClienttMock
                .Setup(x => x.GenerateOAuthRequestUrl(redirectUrl, It.IsAny<string>(), It.IsAny<string>()))
                .Returns("http://generated-url.com");

            // Act
            var result = await googleOAuthService.GenerateOAuthRequestUrlAsync(redirectUrl, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo("http://generated-url.com"));
            googleClienttMock.Verify(x => x.GenerateOAuthRequestUrl(redirectUrl, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            stringVerifierMock.Verify(x => x.GetStringVerifierAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}