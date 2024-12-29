using AuthenticationApi.Dtos;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
    internal class ConfirmEmailControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task ConfirmEmail_ValidRequest_ConfrimsUserEmailAndReturnsAuthData()
        {
            if (isConfirmEmailEnabled)
            {
                // Arrange
                var email = "testuser@example.com";

                await RegisterSampleUser(new UserRegistrationRequest
                {
                    Email = email,
                    Password = "Test@123",
                    ConfirmPassword = "Test@123"
                }, false);

                mockBackgroundJobClient!.Verify
                (
                    x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()),
                    Times.AtLeastOnce
                );

                var user = await userManager.FindByEmailAsync(email);

                Assert.IsNotNull(user);

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                var request = new EmailConfirmationRequest
                {
                    Email = email,
                    Token = token
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/confirmation");
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                // Act
                var httpResponse = await client.SendAsync(httpRequest);

                // Assert
                Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                var content = await httpResponse.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Assert.NotNull(authResponse);
                Assert.That(authResponse.Email, Is.EqualTo(email));

                Assert.NotNull(authResponse.AuthToken);
                Assert.NotNull(authResponse.AuthToken.AccessToken);

            }
            else
            {
                Assert.Pass();
            }
        }

        [Test]
        public async Task ConfirmEmail_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new EmailConfirmationRequest
            {
                Email = "",
                Token = ""
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/confirmation");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task ConfirmEmail_WrongToken_ReturnsConflict()
        {
            if (isConfirmEmailEnabled)
            {
                // Arrange
                var email = "testuser1@example.com";

                await RegisterSampleUser(new UserRegistrationRequest
                {
                    Email = email,
                    Password = "Test@123",
                    ConfirmPassword = "Test@123"
                }, false);

                mockBackgroundJobClient!.Verify
               (
                   x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()),
                   Times.AtLeastOnce
               );

                var user = await userManager.FindByEmailAsync(email);

                Assert.IsNotNull(user);

                var token = "some-wrong-token";

                var request = new EmailConfirmationRequest
                {
                    Email = email,
                    Token = token
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/confirmation");
                httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                // Act
                var httpResponse = await client.SendAsync(httpRequest);

                // Assert
                Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            }
            else
            {
                Assert.Pass();
            }
        }
    }
}
