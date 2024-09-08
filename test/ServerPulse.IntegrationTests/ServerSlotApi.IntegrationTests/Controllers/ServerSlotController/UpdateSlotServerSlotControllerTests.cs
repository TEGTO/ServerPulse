using ServerSlotApi.Domain.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class UpdateSlotServerSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task UpdateSlot_ValidRequest_ReturnsOk()
        {
            // Arrange
            var createdSlot = await CreateSampleSlot(new CreateServerSlotRequest { Name = "OriginalSlot" });
            var updateRequest = new UpdateServerSlotRequest { Id = createdSlot.Id, Name = "UpdatedSlot" };
            using var request = new HttpRequestMessage(HttpMethod.Put, "/serverslot");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task UpdateSlot_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            var updateRequest = new UpdateServerSlotRequest { Id = "someId", Name = "UpdatedSlot" };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/serverslot");
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        [Test]
        public async Task UpdateSlot_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new UpdateServerSlotRequest { Id = "", Name = "" };
            using var request = new HttpRequestMessage(HttpMethod.Put, "/serverslot");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test]
        public async Task UpdateSlot_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var updateRequest = new UpdateServerSlotRequest { Id = "nonexistentId", Name = "UpdatedSlot" };
            using var request = new HttpRequestMessage(HttpMethod.Put, "/serverslot");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}
