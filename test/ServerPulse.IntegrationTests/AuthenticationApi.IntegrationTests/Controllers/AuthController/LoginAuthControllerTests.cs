using AuthenticationApi.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class LoginAuthControllerTests : BaseAuthControllerTest
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await RegisterSampleUser(new UserRegistrationRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            });
        }

        [Test]
        public async Task Login_User_ValidRequest_ReturnsOkWithAuthData()
        {
            // Arrange
            var loginRequest = new UserAuthenticationRequest
            {
                Login = "testuser@example.com",
                Password = "Test@123"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            request.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(authResponse);
            Assert.That(authResponse.Email, Is.EqualTo("testuser@example.com"));

            Assert.NotNull(authResponse.AuthToken);
            Assert.NotNull(authResponse.AuthToken.AccessToken);
        }

        [Test]
        public async Task Login_User_InvalidRequest_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new UserAuthenticationRequest
            {
                Login = "testuser@example.com",
                Password = "WrongPassword" // Invalid
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            request.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}