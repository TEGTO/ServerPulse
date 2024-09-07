using ServerSlotApi.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ServerSlotApi.IntegrationTests.Controllers
{
    internal class GerSlotsContainingStringServerSlotControllerTests : BaseServerSlotControllerTest
    {
        [OneTimeSetUp]
        public async Task SetUp()
        {
            await CreateSamplesAsync();
        }

        [Test]
        public async Task GetSlotsContainingString_Unauthorized_ReturnsUnauthorized()
        {
            // Act 
            var response = await client.GetAsync("/serverslot/contains/Slot");
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        [Test]
        public async Task GetSlotsContainingString_ReturnsOkWithTwoServerSlots()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, "/serverslot/contains/Slot");
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
        public async Task GetSlotsContainingString_ReturnsOkWithOneServerSlots()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, "/serverslot/contains/Slot1");
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
            Assert.That(actualServerSlots.Count(), Is.EqualTo(1));
            Assert.That(actualServerSlots.First().Name, Is.EqualTo("Slot1"));
        }
    }
}
