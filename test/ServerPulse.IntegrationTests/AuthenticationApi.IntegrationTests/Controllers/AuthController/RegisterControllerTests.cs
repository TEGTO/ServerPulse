using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    [TestFixture]
    internal class RegisterControllerTests : BaseAuthControllerTest
    {
        [Test]
        public async Task Register_User_ValidRequest_CreatesUser()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var user = await userManager.FindByEmailAsync(request.Email);

            Assert.NotNull(user);

            Assert.IsTrue(await userManager.CheckPasswordAsync(user, request.Password));

            if (isConfirmEmailEnabled)
            {
                mockBackgroundJobClient!.Verify
                (
                    x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()),
                    Times.AtLeastOnce
                );
            }
            else
            {
                mockBackgroundJobClient!.Verify
                (
                    x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()),
                    Times.Never
                );
            }
        }

        [Test]
        public async Task Register_User_ConflictingEmails_ReturnsConflict()
        {
            // Arrange
            await RegisterSampleUser(new RegisterRequest
            {
                Email = "conflict@example.com",
                Password = "ConflictPassword123",
                ConfirmPassword = "ConflictPassword123"
            });

            var request = new RegisterRequest
            {
                Email = "conflict@example.com",
                Password = "ConflictPassword123",
                ConfirmPassword = "ConflictPassword123"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task Register_User_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "testuser@example.com",
                Password = "Test@123"
                //No confirmation
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/register");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}