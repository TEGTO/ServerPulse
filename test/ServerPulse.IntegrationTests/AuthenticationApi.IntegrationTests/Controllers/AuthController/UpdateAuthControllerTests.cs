using AuthenticationApi.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class UpdateAuthControllerTests : BaseAuthControllerTest
    {
        [OneTimeSetUp]
        protected async Task OneTimeSetUp()
        {
            await RegisterSampleUser(new UserRegistrationRequest
            {
                Email = "olduser1@example.com",
                Password = "123456;QWERTY",
                ConfirmPassword = "123456;QWERTY"
            });
        }

        [Test]
        public async Task UpdateUser_ValidRequest_ReturnsOk()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest
            {
                OldEmail = "olduser1@example.com",
                NewEmail = "updateduser1@example.com",
                OldPassword = "123456;QWERTY",
                NewPassword = "NEW123456;QWERTY"
            };

            var accessKey = await GetAccessKeyForUserAsync(new UserAuthenticationRequest
            {
                Login = "olduser1@example.com",
                Password = "123456;QWERTY",
            });

            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var loginRequest = new UserAuthenticationRequest
            {
                Login = "updateduser1@example.com",
                Password = "NEW123456;QWERTY"
            };

            using var request2 = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            request2.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var httpResponse2 = await client.SendAsync(request2);

            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse2.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(authResponse);
            Assert.That(authResponse.Email, Is.EqualTo("updateduser1@example.com"));
        }

        [Test]
        public async Task UpdateUser_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest
            {
                OldEmail = "olduser1@example.com",
                NewEmail = "updateduser2@example.com",
                OldPassword = "123456;QWERTY",
                NewPassword = "NEW123456;QWERTY"
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task UpdateUser_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest
            {
                OldEmail = "",
                NewEmail = "updateduser2@example.com",
                OldPassword = "",
                NewPassword = "NEW123456;QWERTY"
            };

            var accessKey = await GetAccessKeyForUserAsync(new UserAuthenticationRequest
            {
                Login = "olduser1@example.com",
                Password = "123456;QWERTY",
            });

            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateUser_ConflictingEmail_ReturnsConflictError()
        {
            // Arrange
            await RegisterSampleUser(new UserRegistrationRequest
            {
                Email = "conflict@example.com",
                Password = "123456;QWERTY",
                ConfirmPassword = "123456;QWERTY"
            });

            await RegisterSampleUser(new UserRegistrationRequest
            {
                Email = "conflict2@example.com",
                Password = "123456;QWERTY",
                ConfirmPassword = "123456;QWERTY"
            });

            var updateRequest = new UserUpdateDataRequest
            {
                OldEmail = "conflict@example.com",  // Conflicting email with another user
                NewEmail = "conflict2@example.com",
                OldPassword = "123456;QWERTY",
                NewPassword = "123456;QWERTY"
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            var accessKey = await GetAccessKeyForUserAsync(new UserAuthenticationRequest
            {
                Login = "conflict@example.com",
                Password = "123456;QWERTY",
            });

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }
    }
}
