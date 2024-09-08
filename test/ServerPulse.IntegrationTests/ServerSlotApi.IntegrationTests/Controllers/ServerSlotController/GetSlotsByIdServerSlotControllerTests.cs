using ServerSlotApi.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class GetSlotsByIdServerSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task GetSlotById_ValidId_ReturnsOKWithItem()
        {
            // Arrange
            var list = await CreateSamplesAsync();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/serverslot/{list[0].Id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var actualServerSlot = JsonSerializer.Deserialize<ServerSlotResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(actualServerSlot);
            Assert.That(actualServerSlot.Name, Is.EqualTo("Slot1"));
        }
        [Test]
        public async Task GetSlotById_Unauthorized_ReturnsUnauthorized()
        {
            // Act 
            var response = await client.GetAsync("/serverslot/1");
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        [Test]
        public async Task GetSlotById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, "/serverslot/1");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
