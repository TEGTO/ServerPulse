﻿using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.CreateSlot;
using System.Net;
using System.Net.Http.Headers;

namespace ServerSlotApi.IntegrationTests.Controllers.ServerSlotController
{
    internal class DeleteSlotControllerTests : BaseServerSlotControllerTest
    {
        [Test]
        public async Task DeleteSlot_ValidId_DeletesSlot()
        {
            // Arrange
            var createdSlot = await CreateSampleSlot(new CreateSlotRequest { Name = "SlotToDelete" }, AccessToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/serverslot/{createdSlot.Id}");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            await CheckServerSlotNotFound(createdSlot.Id ?? "");
        }

        [Test]
        public async Task DeleteSlot_UnauthorizedRequest_ReturnsUnauthorized()
        {
            // Arrange
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/serverslot/validId");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task DeleteSlot_NonExistentId_ReturnsOk()
        {
            // Arrange
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/serverslot/nonexistentId");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            await CheckServerSlotNotFound("nonexistentId");
        }

        private async Task CheckServerSlotNotFound(string id)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/serverslot/{id}");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var httpResponse = await client.SendAsync(httpRequest);

            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
