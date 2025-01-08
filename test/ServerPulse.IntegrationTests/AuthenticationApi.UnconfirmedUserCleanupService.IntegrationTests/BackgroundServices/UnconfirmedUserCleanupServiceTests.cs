using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Register;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.UnconfirmedUserCleanupService.IntegrationTests.BackgroundServices
{
    [TestFixture]
    internal sealed class UnconfirmedUserCleanupServiceTests : BaseIntegrationTest
    {
        [Test]
        public async Task CleanupUnconfirmedUsersAsync_DeletesUserWithUnconfirmedEmail()
        {
            // Arrange
            var confirmedEmail = "confirmed@example.com";
            var unConfirmedEmail = "unconfirmed@example.com";

            // Act
            await RegisterSampleUser(new RegisterRequest
            {
                Email = confirmedEmail,
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            }, true);

            await RegisterSampleUser(new RegisterRequest
            {
                Email = unConfirmedEmail,
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            }, false);

            // Assert

            await Utility.WaitUntil(async () =>
            {
                var confirmedUser = await userManager.FindByEmailAsync(confirmedEmail);
                var unconfirmedUser = await userManager.FindByEmailAsync(unConfirmedEmail);

                if (isConfirmEmailEnabled)
                {
                    return confirmedUser != null && unconfirmedUser == null;
                }
                else
                {
                    return confirmedUser != null && unconfirmedUser != null;
                }
            }, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10));

            var finalConfirmedUser = await userManager.FindByEmailAsync(confirmedEmail);
            var finalUnconfirmedUser = await userManager.FindByEmailAsync(unConfirmedEmail);

            if (isConfirmEmailEnabled)
            {
                Assert.IsNotNull(finalConfirmedUser);
                Assert.Null(finalUnconfirmedUser);
            }
            else
            {
                Assert.IsNotNull(finalConfirmedUser);
                Assert.IsNotNull(finalUnconfirmedUser);
            }
        }

        private async Task RegisterSampleUser(RegisterRequest request, bool confirmEmail)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/register");

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var httpResponse = await client.SendAsync(httpRequest);
            httpResponse.EnsureSuccessStatusCode();

            if (isConfirmEmailEnabled && confirmEmail)
            {
                var user = await userManager.FindByEmailAsync(request.Email);

                ArgumentNullException.ThrowIfNull(user);

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                await userManager.ConfirmEmailAsync(user, token);
            }
        }
    }
}
