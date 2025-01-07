using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Register;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
    internal class ConfirmEmailControllerTests : BaseAuthControllerTest
    {
        [SetUp]
        public void TestSetUp()
        {
            if (!isConfirmEmailEnabled)
            {
                Assert.Ignore("Email confirmation feature is disabled. Skipping test.");
            }
        }

        [Test]
        public async Task ConfirmEmail_ValidRequest_ConfrimsUserEmailAndReturnsAuthData()
        {
            // Arrange
            var email = "testuser@example.com";

            await RegisterSampleUser(new RegisterRequest
            {
                Email = email,
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            }, false);

            mockBackgroundJobClient!.Verify
            (
                x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()),
                Times.AtLeastOnce
            );

            var user = await userManager.FindByEmailAsync(email);

            Assert.IsNotNull(user);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var request = new ConfirmEmailRequest
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
            var authResponse = JsonSerializer.Deserialize<ConfirmEmailResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(authResponse);
            Assert.That(authResponse.Email, Is.EqualTo(email));

            Assert.NotNull(authResponse.AccessTokenData);
            Assert.NotNull(authResponse.AccessTokenData.AccessToken);
        }

        [Test]
        public async Task ConfirmEmail_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new ConfirmEmailRequest
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
            // Arrange
            var email = "testuser1@example.com";

            await RegisterSampleUser(new RegisterRequest
            {
                Email = email,
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            }, false);

            mockBackgroundJobClient!.Verify
           (
               x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()),
               Times.AtLeastOnce
           );

            var user = await userManager.FindByEmailAsync(email);

            Assert.IsNotNull(user);

            var token = "some-wrong-token";

            var request = new ConfirmEmailRequest
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
    }
}
