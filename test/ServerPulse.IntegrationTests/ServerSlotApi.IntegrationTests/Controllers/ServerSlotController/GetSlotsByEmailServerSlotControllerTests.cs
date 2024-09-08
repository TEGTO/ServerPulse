using ServerSlotApi.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class GetSlotsByEmailServerSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task GetSlotsByEmail_ReturnsOkWithServerSlots()
        {
            // Arrange
            await CreateSamplesAsync();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/serverslot");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            // Act 
            var response = await client.SendAsync(request);
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var actualServerSlots = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(actualServerSlots);
            Assert.That(actualServerSlots.Count(), Is.EqualTo(2));
            Assert.That(actualServerSlots.First().Name, Is.EqualTo("Slot2"));
        }
        [Test]
        public async Task GetSlotsByEmail_Unauthorized_ReturnsUnauthorized()
        {
            // Act 
            var response = await client.GetAsync("/serverslot");
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}