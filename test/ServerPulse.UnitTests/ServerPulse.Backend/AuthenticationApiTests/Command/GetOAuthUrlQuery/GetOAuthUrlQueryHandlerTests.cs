using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Services;
using Moq;

namespace AuthenticationApi.Command.GetOAuthUrlQuery.Tests
{
    [TestFixture]
    internal class GetOAuthUrlQueryHandlerTests
    {
        private Mock<IOAuthService> mockGoogleOAuthService;
        private Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;
        private GetOAuthUrlQueryHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockGoogleOAuthService = new Mock<IOAuthService>();

            oAuthServices = new Dictionary<OAuthLoginProvider, IOAuthService>
            {
                { OAuthLoginProvider.Google, mockGoogleOAuthService.Object },
            };

            handler = new GetOAuthUrlQueryHandler(oAuthServices);
        }

        [Test]
        public async Task Handle_GoogleOAuthProvider_ReturnsCorrectUrl()
        {
            // Arrange
            string expectedUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=test-client-id";

            var request = new GetOAuthUrlQuery
            (
                new GetOAuthUrlQueryParams
                {
                    OAuthLoginProvider = OAuthLoginProvider.Google,
                    RedirectUrl = "https://example.com/callback",
                    CodeVerifier = "google-code-verifier"
                }
            );

            mockGoogleOAuthService.Setup(service => service.GenerateOAuthRequestUrl(It.IsAny<OAuthRequestUrlParams>()))
                .Returns(expectedUrl);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Url, Is.EqualTo(expectedUrl));
        }

        [Test]
        public void Handle_InvalidOAuthProvider_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new GetOAuthUrlQuery
            (
                new GetOAuthUrlQueryParams
                {
                    OAuthLoginProvider = (OAuthLoginProvider)999,
                    RedirectUrl = "https://example.com/callback",
                    CodeVerifier = "invalid-provider-verifier"
                }
            );

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await handler.Handle(request, CancellationToken.None);
            });
        }
    }
}