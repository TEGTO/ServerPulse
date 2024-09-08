using AuthenticationApi.Domain.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class UpdateAuthControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task Update_User_ValidRequest_ReturnsOk()
        {
            // Arrange
            await RegisterSampleUser(new UserRegistrationRequest
            {
                UserName = "testuser1",
                Email = "olduser1@example.com",
                Password = "OldPassword123",
                ConfirmPassword = "OldPassword123"
            });
            var updateRequest = new UserUpdateDataRequest
            {
                UserName = "updateduser1",
                OldEmail = "olduser1@example.com",
                NewEmail = "updateduser1@example.com",
                OldPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };
            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task Update_User_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest
            {
                UserName = "updateduser",
                OldEmail = "olduser@example.com",
                NewEmail = "updateduser@example.com",
                OldPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };
            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        [Test]
        public async Task Update_User_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest
            {
                UserName = "updateduser",
                OldEmail = null,  // Invalid: Missing OldEmail
                NewEmail = "updateduser@example.com",
                OldPassword = "OldPassword123",
                NewPassword = null  // Invalid: Missing NewPassword
            };
            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task Update_User_ConflictingEmail_ReturnsConflict()
        {
            // Arrange
            await RegisterSampleUser(new UserRegistrationRequest
            {
                UserName = "conflictuser",
                Email = "conflict@example.com",
                Password = "ConflictPassword123",
                ConfirmPassword = "ConflictPassword123"
            });
            await RegisterSampleUser(new UserRegistrationRequest
            {
                UserName = "testuser2",
                Email = "olduser2@example.com",
                Password = "OldPassword123",
                ConfirmPassword = "OldPassword123"
            });
            var updateRequest = new UserUpdateDataRequest
            {
                UserName = "updateduser2",
                OldEmail = "olduser2@example.com",
                NewEmail = "conflict@example.com",  // Conflicting email with another user
                OldPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };
            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
