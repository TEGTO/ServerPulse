using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class RefreshTokenAuthControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task RefreshToken_ValidRequest_ReturnsOk()
        {
            // Arrange
            var refreshRequest = await GetValidAuthToken();
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
            request.Content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        private async Task<AuthToken> GetValidAuthToken()
        {
            await RegisterSampleUser(new UserRegistrationRequest
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            });
            var loginRequest = new UserAuthenticationRequest
            {
                Login = "testuser",
                Password = "Test@123"
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            request.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return authResponse.AuthToken;
        }
    }
}
