using ServerSlotApi.Dtos;
using Shared.Dtos.ServerSlot;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class CheckSlotKeyServerSlotControllerTests : BaseServerSlotControllerTest
    {
        private List<ServerSlotResponse> serverSlotReponses;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            serverSlotReponses = await CreateSamplesAsync();
        }

        [Test]
        public async Task CheckSlotKey_ValidKey_ReturnsOKWithValidResponse()
        {
            // Arrange
            var checkRequest = new CheckSlotKeyRequest { SlotKey = serverSlotReponses[0].SlotKey };
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/check");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(
              JsonSerializer.Serialize(checkRequest),
              Encoding.UTF8,
              "application/json"
            );
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var checkResponse = JsonSerializer.Deserialize<CheckSlotKeyResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(checkResponse);
            Assert.That(checkResponse.SlotKey, Is.EqualTo(serverSlotReponses[0].SlotKey));
            Assert.True(checkResponse.IsExisting);
        }
        [Test]
        public async Task CheckSlotKey_InvalidRequest_ReturnsValidationError()
        {
            // Arrange
            var checkRequest = new CheckSlotKeyRequest { SlotKey = null };
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/check");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(
              JsonSerializer.Serialize(checkRequest),
              Encoding.UTF8,
              "application/json"
            );
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task CheckSlotKey_InvalidKey_ReturnsOKWithResponseThatItemNotExist()
        {
            // Arrange
            var checkRequest = new CheckSlotKeyRequest { SlotKey = "invalidKey" };
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/check");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(
              JsonSerializer.Serialize(checkRequest),
              Encoding.UTF8,
              "application/json"
            );
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var checkResponse = JsonSerializer.Deserialize<CheckSlotKeyResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(checkResponse);
            Assert.That(checkResponse.SlotKey, Is.EqualTo("invalidKey"));
            Assert.False(checkResponse.IsExisting);
        }
    }
}