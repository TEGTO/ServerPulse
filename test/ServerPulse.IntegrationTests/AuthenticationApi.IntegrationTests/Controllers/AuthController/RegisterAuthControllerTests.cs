using AuthenticationApi.Domain.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class RegisterAuthControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task Register_User_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var registrationRequest = new UserRegistrationRequest
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            request.Content = new StringContent(JsonSerializer.Serialize(registrationRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }
        [Test]
        public async Task Register_User_ConflictingEmail_ReturnsConflict()
        {
            // Arrange
            await RegisterSampleUser(new UserRegistrationRequest
            {
                UserName = "conflictuser",
                Email = "conflict@example.com",
                Password = "ConflictPassword123",
                ConfirmPassword = "ConflictPassword123"
            });
            var registrationRequest = new UserRegistrationRequest
            {
                UserName = "conflictuser",
                Email = "conflict@example.com",
                Password = "ConflictPassword123",
                ConfirmPassword = "ConflictPassword123"
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            request.Content = new StringContent(JsonSerializer.Serialize(registrationRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task Register_User_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var registrationRequest = new UserRegistrationRequest
            {
                UserName = "", // Invalid
                Email = "testuser@example.com",
                Password = "Test@123"
                //No confirmation
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            request.Content = new StringContent(JsonSerializer.Serialize(registrationRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}