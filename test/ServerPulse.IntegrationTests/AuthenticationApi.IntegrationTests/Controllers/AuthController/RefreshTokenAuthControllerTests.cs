using AuthenticationApi.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class RefreshTokenAuthControllerTests : BaseAuthControllerTest
    {
        private async Task<AuthToken> GetValidAuthToken()
        {
            await RegisterSampleUser(new UserRegistrationRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            });

            var loginRequest = new UserAuthenticationRequest
            {
                Login = "testuser@example.com",
                Password = "Test@123"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            request.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(request);
            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return authResponse?.AuthToken ?? throw new InvalidOperationException("Can't get valid auth token");
        }

        [Test]
        public async Task RefreshToken_ValidRequest_ReturnsOkWithRefreshedToken()
        {
            // Arrange
            var refreshRequest = await GetValidAuthToken();
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");

            request.Content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<AuthToken>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.AccessToken);
            Assert.IsNotNull(response.RefreshToken);
            Assert.That(response.RefreshToken, Is.Not.EqualTo(refreshRequest.RefreshToken));
        }

        [Test]
        public async Task RefreshToken_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var refreshRequest = new AuthToken
            {
                AccessToken = "",
                RefreshToken = "validRefreshToken"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
            request.Content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
