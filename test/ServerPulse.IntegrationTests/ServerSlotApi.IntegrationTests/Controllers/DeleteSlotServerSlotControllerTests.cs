using ServerSlotApi.Domain.Dtos;
using System.Net;
using System.Net.Http.Headers;

namespace ServerSlotApi.IntegrationTests.Controllers
{
    internal class DeleteSlotServerSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task DeleteSlot_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/serverslot/validId");
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        [Test]
        public async Task DeleteSlot_ValidId_ReturnsOk()
        {
            // Arrange
            var createdSlot = await CreateSampleSlot(new CreateServerSlotRequest { Name = "SlotToDelete" });
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/serverslot/{createdSlot.Id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            await CheckServerSlotNotFound(createdSlot.Id);
        }
        [Test]
        public async Task DeleteSlot_NonExistentId_ReturnsOk()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Delete, "/serverslot/nonexistentId");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            // Act
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            await CheckServerSlotNotFound("nonexistentId");
        }

        private async Task CheckServerSlotNotFound(string id)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/serverslot/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await client.SendAsync(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
