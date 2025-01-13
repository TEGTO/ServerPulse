using AuthenticationApi.Core.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.UserUpdate;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
    internal class UpdateAuthControllerTests : BaseAuthControllerTest
    {
        [OneTimeSetUp]
        protected async Task OneTimeSetUp()
        {
            await RegisterSampleUser(new RegisterRequest
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
            var request = new UserUpdateRequest
            {
                Email = "updateduser1@example.com",
                OldPassword = "123456;QWERTY",
                Password = "NEW123456;QWERTY"
            };

            var accessKey = await GetAccessKeyForUserAsync(new LoginRequest
            {
                Login = "olduser1@example.com",
                Password = "123456;QWERTY",
            });

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var loginRequest = new LoginRequest
            {
                Login = "updateduser1@example.com",
                Password = "NEW123456;QWERTY"
            };

            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            httpRequest2.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var httpResponse2 = await client.SendAsync(httpRequest2);

            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse2.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(authResponse);
            Assert.That(authResponse.Email, Is.EqualTo("updateduser1@example.com"));
        }

        [Test]
        public async Task UpdateUser_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            var request = new UserUpdateRequest
            {
                Email = "updateduser2@example.com",
                OldPassword = "123456;QWERTY",
                Password = "NEW123456;QWERTY"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task UpdateUser_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new UserUpdateRequest
            {
                Email = "",
                OldPassword = "",
                Password = "NEW123456;QWERTY"
            };

            var accessKey = await GetAccessKeyForUserAsync(new LoginRequest
            {
                Login = "olduser1@example.com",
                Password = "123456;QWERTY",
            });

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/auth/update");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
