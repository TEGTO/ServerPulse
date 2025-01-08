using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Register;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
    internal class LoginControllerTests : BaseAuthControllerTest
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await RegisterSampleUser(new RegisterRequest
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
            var request = new LoginRequest
            {
                Login = "testuser@example.com",
                Password = "Test@123"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(authResponse);
            Assert.That(authResponse.Email, Is.EqualTo("testuser@example.com"));

            Assert.NotNull(authResponse.AccessTokenData);
            Assert.NotNull(authResponse.AccessTokenData.AccessToken);
        }

        [Test]
        public async Task Login_User_InvalidRequest_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Login = "testuser@example.com",
                Password = "WrongPassword" // Invalid
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Login_User_EmailIsNotConfirmed_ReturnsUnauthorized()
        {
            if (isConfirmEmailEnabled)
            {
                // Arrange
                await RegisterSampleUser(new RegisterRequest
                {
                    Email = "testuser1@example.com",
                    Password = "Test@123",
                    ConfirmPassword = "Test@123"
                }, false);

                var request = new LoginRequest
                {
                    Login = "testuser1@example.com",
                    Password = "Test@123"
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                // Act
                var httpResponse = await client.SendAsync(httpRequest);

                // Assert
                Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            }
            else
            {
                Assert.Ignore("Email confirmation feature is disabled. Skipping test.");
            }
        }
    }
}