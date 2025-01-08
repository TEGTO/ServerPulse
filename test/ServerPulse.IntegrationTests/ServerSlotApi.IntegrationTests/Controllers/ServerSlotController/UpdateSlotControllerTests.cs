using ServerSlotApi.Dtos.Endpoints.ServerSlot.CreateSlot;
using ServerSlotApi.Dtos.Endpoints.Slot.GetSlotById;
using ServerSlotApi.Dtos.Endpoints.Slot.UpdateSlot;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class UpdateSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task UpdateSlot_ValidRequest_UpdatesSlot()
        {
            // Arrange
            var createdSlot = await CreateSampleSlot(new CreateSlotRequest { Name = "OriginalSlot" }, AccessToken);
            var slotId = createdSlot.Id ?? "";

            var request = new UpdateSlotRequest { Id = slotId, Name = "UpdatedSlot" };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/serverslot");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);
            var updatedSlot = await GetSlotByIdAsync(slotId);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updatedSlot.Name, Is.EqualTo("UpdatedSlot"));
        }

        [Test]
        public async Task UpdateSlot_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            var request = new UpdateSlotRequest { Id = "someId", Name = "UpdatedSlot" };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/serverslot");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task UpdateSlot_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateSlotRequest { Id = "", Name = "" };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/serverslot");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateSlot_NonExistentId_ReturnsConflict()
        {
            // Arrange
            var request = new UpdateSlotRequest { Id = "nonexistentId", Name = "UpdatedSlot" };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/serverslot");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        public async Task<GetSlotByIdResponse> GetSlotByIdAsync(string id)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/serverslot/{id}");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var httpResponse = await client.SendAsync(httpRequest);

            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();

            var response = JsonSerializer.Deserialize<GetSlotByIdResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ArgumentNullException.ThrowIfNull(response);

            return response;
        }
    }
}
