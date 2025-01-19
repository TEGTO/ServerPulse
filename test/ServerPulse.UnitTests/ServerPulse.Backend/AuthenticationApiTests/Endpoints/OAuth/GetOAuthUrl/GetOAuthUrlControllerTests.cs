using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.OAuth.GetOAuthUrl;
using AuthenticationApi.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthenticationApi.Endpoints.OAuth.GetOAuthUrl.Tests
{
    [TestFixture]
    internal class GetOAuthUrlControllerTests
    {
        private Mock<IOAuthService> mockGoogleOAuthService;
        private Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;
        private GetOAuthUrlController controller;

        [SetUp]
        public void SetUp()
        {
            mockGoogleOAuthService = new Mock<IOAuthService>();

            oAuthServices = new Dictionary<OAuthLoginProvider, IOAuthService>
            {
                { OAuthLoginProvider.Google, mockGoogleOAuthService.Object },
            };

            controller = new GetOAuthUrlController(oAuthServices);
        }

        [Test]
        public async Task GetOAuthUrlAsync_ValidRequest_ReturnsOkWithCorrectUrl()
        {
            // Arrange
            var expectedUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=test-client-id";

            var request = new GetOAuthUrlParams
            {
                OAuthLoginProvider = OAuthLoginProvider.Google,
                RedirectUrl = "https://example.com/callback",
            };

            mockGoogleOAuthService.Setup(service => service.GenerateOAuthRequestUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUrl);

            // Act
            var result = await controller.GetOAuthUrlAsync(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var response = (result.Result as OkObjectResult)?.Value as GetOAuthUrlResponse;

            Assert.NotNull(response);
            Assert.That(response.Url, Is.EqualTo(expectedUrl));
        }

        [Test]
        public void GetOAuthUrlAsync_InvalidOAuthProvider_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new GetOAuthUrlParams
            {
                OAuthLoginProvider = (OAuthLoginProvider)999,
                RedirectUrl = "https://example.com/callback",
            };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await controller.GetOAuthUrlAsync(request, CancellationToken.None);
            });
        }
    }
}