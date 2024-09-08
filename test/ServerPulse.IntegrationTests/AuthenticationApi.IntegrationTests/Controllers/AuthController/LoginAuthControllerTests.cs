using AuthenticationApi.Domain.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class LoginAuthControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task Login_User_ValidRequest_ReturnsOk()
        {
            // Arrange
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
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(authResponse);
            Assert.That(authResponse.Email, Is.EqualTo("testuser@example.com"));
        }
        [Test]
        public async Task Login_User_InvalidRequest_ReturnsUnauthorized()
        {
            // Arrange
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
                Password = "WrongPassword" // Invalid
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            request.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}