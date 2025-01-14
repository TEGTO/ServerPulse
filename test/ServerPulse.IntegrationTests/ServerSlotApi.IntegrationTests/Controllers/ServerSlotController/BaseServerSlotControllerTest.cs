using Authentication.Models;
using Authentication.Token;
using Microsoft.Extensions.Options;
using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.CreateSlot;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class BaseServerSlotControllerTest : BaseIntegrationTest
    {
        private string accessToken = string.Empty;

        protected string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = GetAccessTokenData("testuser", "test@example.com").AccessToken;
                }
                return accessToken;
            }
        }

        protected AccessTokenData GetAccessTokenData(string userName, string email)
        {
            var options = Options.Create(settings);

            var jwtHandler = new JwtHandler(options);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier,  userName + email),
            };

            return jwtHandler.CreateToken(claims);
        }

        protected async Task<List<CreateSlotResponse>> CreateSamplesSlotsAsync(string accessToken)
        {
            var requests = new List<CreateSlotRequest>
            {
                new CreateSlotRequest { Name = "Slot1" },
                new CreateSlotRequest { Name = "Slot2" }
            };

            var responseSlots = new List<CreateSlotResponse>
            {
                await CreateSampleSlot(requests[0], accessToken),
                await CreateSampleSlot(requests[1], accessToken)
            };

            return responseSlots;
        }

        protected async Task<CreateSlotResponse> CreateSampleSlot(CreateSlotRequest request, string accessToken)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverslot");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var httpResponse = await client.SendAsync(httpRequest);

            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();

            var response = JsonSerializer.Deserialize<CreateSlotResponse?>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ArgumentNullException.ThrowIfNull(response);

            return response;
        }
    }
}
