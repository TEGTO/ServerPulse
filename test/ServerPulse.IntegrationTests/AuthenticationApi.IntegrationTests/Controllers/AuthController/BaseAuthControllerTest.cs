using Authentication.Models;
using Authentication.Services;
using AuthenticationApi.Domain.Dtos;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Text.Json;

namespace AuthenticationApi.IntegrationTests.Controllers.AuthController
{
    internal class BaseAuthControllerTest : BaseIntegrationTest
    {
        private string accessToken;

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
            var jwtHandler = new JwtHandler(settings);
            IdentityUser identity = new IdentityUser()
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            return jwtHandler.CreateToken(identity);
        }
        protected async Task RegisterSampleUser(UserRegistrationRequest registerRequest)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");

            request.Content = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.SendAsync(request);
        }
    }
}
