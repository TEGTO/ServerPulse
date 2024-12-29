using AuthenticationApi.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
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
            var request = new UserAuthenticationRequest
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
            var request = new UserAuthenticationRequest
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
                await RegisterSampleUser(new UserRegistrationRequest
                {
                    Email = "testuser1@example.com",
                    Password = "Test@123",
                    ConfirmPassword = "Test@123"
                }, false);

                var request = new UserAuthenticationRequest
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
                Assert.Pass();
            }
        }
    }
}