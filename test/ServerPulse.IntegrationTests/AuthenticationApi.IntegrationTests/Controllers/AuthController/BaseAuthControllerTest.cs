using Authentication.Models;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using System.Security.Claims;
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
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name,  "testuser"),
            };

            return tokenHandler.CreateToken(claims);
        }

        protected async Task RegisterSampleUser(RegisterRequest request, bool confirmEmail = true)
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

        protected async Task<string?> GetAccessKeyForUserAsync(LoginRequest request)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(httpRequest);
            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return response?.AccessTokenData?.AccessToken;
        }
    }
}
