using Authentication.Models;
using Authentication.Token;
using AuthenticationApi.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class BaseAuthControllerTest : BaseIntegrationTest
    {
        private string accessToken = string.Empty;

        protected string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = GetAccessTokenData().AccessToken;
                }
                return accessToken;
            }
        }

        protected AccessTokenData GetAccessTokenData()
        {
            var options = Options.Create(settings);

            var jwtHandler = new JwtHandler(options);

            IdentityUser identity = new IdentityUser()
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            return jwtHandler.CreateToken(identity);
        }

        protected async Task RegisterSampleUser(UserRegistrationRequest request, bool confirmEmail = true)
        {
            using var registerHttpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/register");

            registerHttpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var httpResponse = await client.SendAsync(registerHttpRequest);
            httpResponse.EnsureSuccessStatusCode();

            if (isConfirmEmailEnabled && confirmEmail)
            {
                var user = await userManager.FindByEmailAsync(request.Email);

                ArgumentNullException.ThrowIfNull(user);

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                await userManager.ConfirmEmailAsync(user, token);
            }
        }

        protected async Task<string?> GetAccessKeyForUserAsync(UserAuthenticationRequest loginRequest)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            request.Content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(request);
            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<UserAuthenticationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return authResponse?.AuthToken?.AccessToken;
        }
    }
}
