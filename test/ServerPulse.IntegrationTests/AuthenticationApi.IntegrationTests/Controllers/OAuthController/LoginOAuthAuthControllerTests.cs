using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.IntegrationTests.Controllers.AuthController;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.OAuthController
{
    internal class LoginOAuthAuthControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task LoginOAuthRequest_ValidRequest_ReturnsOk()
        {
            //Arrange
            var request = new UserOAuthenticationRequest
            {
                Code = "somecode",
                CodeVerifier = "someverifier",
                RedirectUrl = "someurl",
                OAuthLoginProvider = OAuthLoginProvider.Google,
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/oauth");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Email, Is.EqualTo("someemail@gmail.com"));
        }

        [Test]
        public async Task LoginOAuthRequest_WrongRequestData_ReturnsBadRequest()
        {
            //Arrange
            var request = new UserOAuthenticationRequest
            {
                Code = "someinvalidcode",
                CodeVerifier = "someinvalidverifier",
                RedirectUrl = "someinvalidurl",
                OAuthLoginProvider = OAuthLoginProvider.Google,
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/oauth");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task LoginOAuthRequest_InvalidRequest_ReturnsBadRequest()
        {
            //Arrange
            var request = new UserOAuthenticationRequest
            {
                Code = "",
                CodeVerifier = "",
                RedirectUrl = "",
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/oauth");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
