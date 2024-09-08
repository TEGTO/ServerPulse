using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class CreateSlotServerSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task CreateSlot_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var createRequest = new CreateServerSlotRequest { Name = "ValidSlot" };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverslot");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var content = await response.Content.ReadAsStringAsync();
            var createdSlot = JsonSerializer.Deserialize<ServerSlotResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(createdSlot);
            Assert.That(createdSlot.Name, Is.EqualTo("ValidSlot"));
            Assert.That(createdSlot.UserEmail, Is.EqualTo("test@example.com"));
        }
        [Test]
        public async Task CreateSlot_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            var createRequest = new CreateServerSlotRequest { Name = "UnauthorizedSlot" };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverslot");
            request.Content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        [Test]
        public async Task CreateSlot_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateServerSlotRequest { Name = "" };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/serverslot");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
