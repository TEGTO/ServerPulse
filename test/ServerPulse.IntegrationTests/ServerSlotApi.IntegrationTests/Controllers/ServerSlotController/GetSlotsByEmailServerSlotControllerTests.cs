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
            await CreateSamplesSlotsAsync(AccessToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/serverslot");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(response);
            Assert.That(response.Count, Is.EqualTo(2));
            Assert.That(response[0].Name, Is.EqualTo("Slot2"));
        }

        [Test]
        public async Task GetSlotsByEmail_ReturnsServerSlotsCachedByUser()
        {
            // Arrange
            var anotherAccessToken = GetAccessTokenData("some other user", "some other email").AccessToken;

            await CreateSamplesSlotsAsync(anotherAccessToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/serverslot");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Get, "/serverslot");
            httpRequest2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            using var httpRequest3 = new HttpRequestMessage(HttpMethod.Get, "/serverslot");
            httpRequest3.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            await CreateSampleSlot(new CreateServerSlotRequest { Name = "NewSlot" }, anotherAccessToken);

            var httpResponse2 = await client.SendAsync(httpRequest2);

            await Task.Delay(2500);

            var httpResponse3 = await client.SendAsync(httpRequest3);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse3.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content2 = await httpResponse2.Content.ReadAsStringAsync();
            var response2 = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content2, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content3 = await httpResponse3.Content.ReadAsStringAsync();
            var response3 = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content3, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(response);
            Assert.NotNull(response2);
            Assert.NotNull(response3);

            Assert.That(response.Count, Is.EqualTo(2));
            Assert.That(response2.Count, Is.EqualTo(2));
            Assert.That(response3.Count, Is.EqualTo(3));

            Assert.That(response[0].Name, Is.EqualTo("Slot2"));
            Assert.That(response2[0].Name, Is.EqualTo("Slot2"));
            Assert.That(response3[0].Name, Is.EqualTo("NewSlot"));
        }

        [Test]
        public async Task GetSlotsByEmail_Unauthorized_ReturnsUnauthorized()
        {
            // Act 
            var httpResponse = await client.GetAsync("/serverslot");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetSlotsByEmail_ProvidedIncludeString_ReturnsOkWithTwoServerSlots()
        {
            // Arrange
            var anotherAccessToken = GetAccessTokenData("some include", "some include").AccessToken;

            await CreateSamplesSlotsAsync(anotherAccessToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/serverslot?contains=Slot");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var actualServerSlots = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(actualServerSlots);
            Assert.That(actualServerSlots.Count, Is.EqualTo(2));
            Assert.That(actualServerSlots[0].Name, Is.EqualTo("Slot2"));
        }

        [Test]
        public async Task GetSlotsByEmail_UnauthorizedProvidedIncludeString_ReturnsUnauthorized()
        {
            // Act 
            var httpResponse = await client.GetAsync("/serverslot?contains=Slot");

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetSlotsByEmail_ProvidedIncludeString_ReturnsOkWithOneServerSlots()
        {
            // Arrange
            var anotherAccessToken = GetAccessTokenData("some", "some").AccessToken;

            await CreateSamplesSlotsAsync(anotherAccessToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/serverslot?contains=Slot1");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var actualServerSlots = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(actualServerSlots);
            Assert.That(actualServerSlots.Count, Is.EqualTo(1));
            Assert.That(actualServerSlots[0].Name, Is.EqualTo("Slot1"));
        }

        [Test]
        public async Task GetSlotsByEmail__ProvidedIncludeString_ReturnsServerSlotsCachedByUser()
        {
            // Arrange
            var anotherAccessToken = GetAccessTokenData("some contains", "some contains").AccessToken;

            await CreateSamplesSlotsAsync(anotherAccessToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/serverslot?contains=Slot1");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            using var httpRequest2 = new HttpRequestMessage(HttpMethod.Get, "/serverslot?contains=Slot1");
            httpRequest2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            using var httpRequest3 = new HttpRequestMessage(HttpMethod.Get, "/serverslot?contains=Slot1");
            httpRequest3.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherAccessToken);

            // Act 
            var httpResponse = await client.SendAsync(httpRequest);

            await CreateSampleSlot(new CreateServerSlotRequest { Name = "Slot1New" }, anotherAccessToken);

            var httpResponse2 = await client.SendAsync(httpRequest2);

            await Task.Delay(2500);

            var httpResponse3 = await client.SendAsync(httpRequest3);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(httpResponse3.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content2 = await httpResponse2.Content.ReadAsStringAsync();
            var response2 = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content2, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var content3 = await httpResponse3.Content.ReadAsStringAsync();
            var response3 = JsonSerializer.Deserialize<List<ServerSlotResponse>>(content3, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(response);
            Assert.NotNull(response2);
            Assert.NotNull(response3);

            Assert.That(response.Count, Is.EqualTo(1));
            Assert.That(response2.Count, Is.EqualTo(1));
            Assert.That(response3.Count, Is.EqualTo(2));

            Assert.That(response[0].Name, Is.EqualTo("Slot1"));
            Assert.That(response2[0].Name, Is.EqualTo("Slot1"));
            Assert.That(response3[0].Name, Is.EqualTo("Slot1New"));
        }
    }
}