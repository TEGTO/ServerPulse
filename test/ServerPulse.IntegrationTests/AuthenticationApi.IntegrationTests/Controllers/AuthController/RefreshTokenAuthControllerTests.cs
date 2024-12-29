using AuthenticationApi.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
    internal class RefreshTokenAuthControllerTests : BaseAuthControllerTest
    {
        private async Task<AccessTokenDataDto> GetValidAuthToken()
        {
            await RegisterSampleUser(new UserRegistrationRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            });

            var request = new UserAuthenticationRequest
            {
                Login = "testuser@example.com",
                Password = "Test@123"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(httpRequest);
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
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");

            httpRequest.Content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<AccessTokenDataDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.AccessToken);
            Assert.IsNotNull(response.RefreshToken);
        }

        [Test]
        public async Task RefreshToken_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var refreshRequest = new AccessTokenDataDto
            {
                AccessToken = "",
                RefreshToken = "validRefreshToken"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
