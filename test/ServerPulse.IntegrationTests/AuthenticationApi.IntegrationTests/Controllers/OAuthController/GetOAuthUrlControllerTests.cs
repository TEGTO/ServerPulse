using AuthenticationApi.Core.Dtos.Endpoints.OAuth.GetOAuthUrl;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.IntegrationTests.Controllers.AuthController;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.OAuthController
{
    [TestFixture]
    internal class GetOAuthUrlControllerTests : BaseAuthControllerTest
    {
        [SetUp]
        public void TestSetUp()
        {
            if (!isOAuthEnabled)
            {
                Assert.Ignore("OAuth feature is disabled. Skipping test.");
            }
        }

        [Test]
        public async Task GetOAuthUrl_ValidRequest_ReturnsOkWithUrl()
        {
            // Arrange
            var requestParams = new GetOAuthUrlParams
            {
                OAuthLoginProvider = OAuthLoginProvider.Google,
                RedirectUrl = "someurl",
                CodeVerifier = "someverifier"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("/oauth",
                new Dictionary<string, string?>
                {
                    {"OAuthLoginProvider", requestParams.OAuthLoginProvider.ToString() },
                    {"RedirectUrl", requestParams.RedirectUrl },
                    {"CodeVerifier", requestParams.CodeVerifier }
                }
            ));

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<GetOAuthUrlResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(response);
            Assert.NotNull(response.Url);
            Assert.IsTrue(response.Url.Contains(requestParams.RedirectUrl));
        }

        [Test]
        public async Task GetOAuthUrl_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var requestParams = new GetOAuthUrlParams
            {
                OAuthLoginProvider = OAuthLoginProvider.Google,
                RedirectUrl = "",
                CodeVerifier = ""
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("/oauth",
                new Dictionary<string, string?>
                {
                    {"OAuthLoginProvider", requestParams.OAuthLoginProvider.ToString() },
                    {"RedirectUrl", requestParams.RedirectUrl },
                    {"CodeVerifier", requestParams.CodeVerifier }
                }
            ));

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}