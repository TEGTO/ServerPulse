using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.CreateSlot;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class CreateSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task CreateSlot_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreateSlotRequest { Name = "ValidSlot" };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverslot");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var createdSlot = JsonSerializer.Deserialize<CreateSlotResponse>(content, new JsonSerializerOptions
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
            var request = new CreateSlotRequest { Name = "UnauthorizedSlot" };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverslot");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task CreateSlot_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateSlotRequest { Name = "" };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/serverslot");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
