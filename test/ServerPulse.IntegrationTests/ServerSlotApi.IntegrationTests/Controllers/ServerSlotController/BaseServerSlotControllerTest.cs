using Authentication.Models;
using Authentication.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ServerSlotApi.Dtos;
using System.Net.Http.Headers;
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

            IdentityUser identity = new IdentityUser()
            {
                UserName = userName,
                Email = email,
                Id = userName + email
            };

            return jwtHandler.CreateToken(identity);
        }

        protected async Task<List<ServerSlotResponse>> CreateSamplesSlotsAsync(string accessToken)
        {
            var requests = new List<CreateServerSlotRequest>
            {
                new CreateServerSlotRequest { Name = "Slot1" },
                new CreateServerSlotRequest { Name = "Slot2" }
            };

            var responseSlots = new List<ServerSlotResponse>
            {
                await CreateSampleSlot(requests[0], accessToken),
                await CreateSampleSlot(requests[1], accessToken)
            };

            return responseSlots;
        }

        protected async Task<ServerSlotResponse> CreateSampleSlot(CreateServerSlotRequest request, string accessToken)
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

            var response = JsonSerializer.Deserialize<ServerSlotResponse?>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ArgumentNullException.ThrowIfNull(response);

            return response;
        }
    }
}
