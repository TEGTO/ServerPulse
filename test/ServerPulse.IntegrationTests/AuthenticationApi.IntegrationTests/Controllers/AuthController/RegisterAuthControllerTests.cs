using AuthenticationApi.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class RegisterAuthControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task Register_User_ValidRequest_ReturnsCreatedUserAuthData()
        {
            // Arrange
            var registrationRequest = new UserRegistrationRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            request.Content = new StringContent(JsonSerializer.Serialize(registrationRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(authResponse);
            Assert.That(authResponse.Email, Is.EqualTo("testuser@example.com"));

            Assert.NotNull(authResponse.AuthToken);
            Assert.NotNull(authResponse.AuthToken.AccessToken);
        }

        [Test]
        public async Task Register_User_ConflictingEmails_ReturnsConflict()
        {
            // Arrange
            await RegisterSampleUser(new UserRegistrationRequest
            {
                Email = "conflict@example.com",
                Password = "ConflictPassword123",
                ConfirmPassword = "ConflictPassword123"
            });

            var registrationRequest = new UserRegistrationRequest
            {
                Email = "conflict@example.com",
                Password = "ConflictPassword123",
                ConfirmPassword = "ConflictPassword123"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            request.Content = new StringContent(JsonSerializer.Serialize(registrationRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task Register_User_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var registrationRequest = new UserRegistrationRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123"
                //No confirmation
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            request.Content = new StringContent(JsonSerializer.Serialize(registrationRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}