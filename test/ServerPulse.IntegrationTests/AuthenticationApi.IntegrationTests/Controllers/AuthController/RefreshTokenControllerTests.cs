using AuthenticationApi.Core.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.RefreshToken;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
    internal class RefreshTokenControllerTests : BaseAuthControllerTest
    {
        private async Task<RefreshTokenRequest> GetRequestWithValidToken()
        {
            await RegisterSampleUser(new RegisterRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            });

            var request = new LoginRequest
            {
                Login = "testuser@example.com",
                Password = "Test@123"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(httpRequest);
            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var tokenDto = response?.AccessTokenData ?? throw new InvalidOperationException("Can't get valid auth token");

            return new RefreshTokenRequest()
            {
                AccessToken = tokenDto.AccessToken,
                RefreshToken = tokenDto.RefreshToken,
                RefreshTokenExpiryDate = tokenDto.RefreshTokenExpiryDate,
            };
        }

        [Test]
        public async Task RefreshToken_ValidRequest_ReturnsOkWithRefreshedToken()
        {
            // Arrange
            var refreshRequest = await GetRequestWithValidToken();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");

            httpRequest.Content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<RefreshTokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.AccessToken);
            Assert.IsNotNull(response.RefreshToken);
        }

        [Test]
        public async Task RefreshToken_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var refreshRequest = new RefreshTokenRequest
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
