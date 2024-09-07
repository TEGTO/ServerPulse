using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Identity;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Dtos;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers
{
    internal class BaseServerSlotControllerTest : BaseIntegrationTest
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
        protected async Task<List<ServerSlotResponse>> CreateSamplesAsync()
        {
            var requests = new List<CreateServerSlotRequest>
            {
                new CreateServerSlotRequest { Name = "Slot1" },
                new CreateServerSlotRequest { Name = "Slot2" }
            };

            var responseSlots = new List<ServerSlotResponse>
            {
                await CreateSampleSlot(requests[0]),
                await CreateSampleSlot(requests[1])
            };

            return responseSlots;
        }
        protected async Task<ServerSlotResponse?> CreateSampleSlot(CreateServerSlotRequest createRequest)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverslot");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            request.Content = new StringContent(
                JsonSerializer.Serialize(createRequest),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ServerSlotResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
