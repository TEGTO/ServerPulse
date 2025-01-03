using ServerSlotApi.Dtos;
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
        public async Task OneTimeSetUp()
        {
            serverSlotReponses = await CreateSamplesSlotsAsync(AccessToken);
        }

        [Test]
        public async Task CheckSlotKey_ValidKey_ReturnsOkWithValidResponse()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = serverSlotReponses[0].SlotKey ?? "" };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/serverslot/check");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(
              JsonSerializer.Serialize(request),
              Encoding.UTF8,
              "application/json"
            );

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<CheckSlotKeyResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(response);
            Assert.That(response.SlotKey, Is.EqualTo(serverSlotReponses[0].SlotKey));
            Assert.True(response.IsExisting);
        }

        [Test]
        public async Task CheckSlotKey_ReturnsOkWithCachedResponse()
        {
            // Arrange
            var slotKey = serverSlotReponses[1].SlotKey ?? "";

            var request = new CheckSlotKeyRequest { SlotKey = slotKey };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/serverslot/check");
            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, $"/serverslot/check");
            using var httpRequest3 = new HttpRequestMessage(HttpMethod.Post, $"/serverslot/check");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(
              JsonSerializer.Serialize(request),
              Encoding.UTF8,
              "application/json"
            );

            httpRequest2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest2.Content = new StringContent(
              JsonSerializer.Serialize(request),
              Encoding.UTF8,
              "application/json"
            );

            httpRequest3.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest3.Content = new StringContent(
              JsonSerializer.Serialize(request),
              Encoding.UTF8,
              "application/json"
            );

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            await DeleteSlotByKeyAsync(serverSlotReponses[1].Id ?? "");

            var httpResponse2 = await client.SendAsync(httpRequest2);

            await Task.Delay(2500);

            var httpResponse3 = await client.SendAsync(httpRequest3);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse3.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<CheckSlotKeyResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content2 = await httpResponse2.Content.ReadAsStringAsync();
            var response2 = JsonSerializer.Deserialize<CheckSlotKeyResponse>(content2, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content3 = await httpResponse3.Content.ReadAsStringAsync();
            var response3 = JsonSerializer.Deserialize<CheckSlotKeyResponse>(content3, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(response);
            Assert.NotNull(response2);
            Assert.NotNull(response3);

            Assert.That(response.SlotKey, Is.EqualTo(serverSlotReponses[1].SlotKey));
            Assert.True(response.IsExisting);

            Assert.That(response2.SlotKey, Is.EqualTo(serverSlotReponses[1].SlotKey));
            Assert.True(response2.IsExisting);

            Assert.That(response3.SlotKey, Is.EqualTo(serverSlotReponses[1].SlotKey));
            Assert.False(response3.IsExisting);
        }

        [Test]
        public async Task CheckSlotKey_InvalidRequest_ReturnsValidationError()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = null! };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/serverslot/check");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(
              JsonSerializer.Serialize(request),
              Encoding.UTF8,
              "application/json"
            );

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task CheckSlotKey_InvalidKey_ReturnsOkWithResponseThatItemNotExist()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = "invalidKey" };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/serverslot/check");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(
              JsonSerializer.Serialize(request),
              Encoding.UTF8,
              "application/json"
            );

            // Act 
            var response = await client.SendAsync(httpRequest);

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

        private async Task DeleteSlotByKeyAsync(string key)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/serverslot/{key}");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var httpResponse = await client.SendAsync(httpRequest);

            httpResponse.EnsureSuccessStatusCode();
        }
    }
}