using AuthenticationApi.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class CheckAuthDataAuthControllerTests : BaseAuthControllerTest
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
        public async Task CheckAuthData_ValidAuthData_ReturnsOkWithAuthDataIsCorrect()
        {
            // Arrange
            var checkAuthRequest = new CheckAuthDataRequest
            {
                Login = "testuser@example.com",
                Password = "Test@123"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/check");
            request.Content = new StringContent(JsonSerializer.Serialize(checkAuthRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var checkAuthResponse = JsonSerializer.Deserialize<CheckAuthDataResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(checkAuthResponse);
            Assert.That(checkAuthResponse.IsCorrect, Is.True);
        }

        [Test]
        public async Task CheckAuthData_InvalidAuthData_ReturnsOkWithAuthDataIsIncorrect()
        {
            // Arrange
            var checkAuthRequest = new CheckAuthDataRequest
            {
                Login = "testuser@example.com",
                Password = "WrongPassword"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/check");
            request.Content = new StringContent(JsonSerializer.Serialize(checkAuthRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var checkAuthResponse = JsonSerializer.Deserialize<CheckAuthDataResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(checkAuthResponse);
            Assert.That(checkAuthResponse.IsCorrect, Is.False);
        }

        [Test]
        public async Task CheckAuthData_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var checkAuthRequest = new CheckAuthDataRequest
            {
                Login = "",
                Password = ""
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/check");
            request.Content = new StringContent(JsonSerializer.Serialize(checkAuthRequest), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(request);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
