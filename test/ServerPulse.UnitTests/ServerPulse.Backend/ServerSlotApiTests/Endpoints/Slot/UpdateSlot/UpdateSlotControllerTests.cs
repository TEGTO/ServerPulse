using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Dtos.Endpoints.Slot.UpdateSlot;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.UpdateSlot.Tests
{
    [TestFixture]
    internal class UpdateSlotControllerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private UpdateSlotController controller;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();

            controller = new UpdateSlotController(repositoryMock.Object, mapperMock.Object);
        }

        [TestCase("", ExpectedResult = typeof(ConflictObjectResult))]
        [TestCase("user@example.com", ExpectedResult = typeof(OkResult))]
        public async Task<Type?> UpdateSlot_EmailValidation_ReturnsExpectedResult(string? email)
        {
            // Arrange
            var request = new UpdateSlotRequest
            {
                Id = "123",
                Name = "Updated Slot Name"
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
              new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            mapperMock.Setup(m => m.Map<ServerSlot>(request))
                .Returns(new ServerSlot());
            repositoryMock.Setup(r => r.GetSlotAsync(It.IsAny<SlotModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServerSlot());

            // Act
            var result = await controller.UpdateSlot(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);

            return result.GetType();
        }

        [Test]
        public async Task UpdateSlot_SlotDoesNotExist_ReturnsConflict()
        {
            // Arrange
            var email = "user@example.com";
            var request = new UpdateSlotRequest
            {
                Id = "123",
                Name = "Updated Slot Name"
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            repositoryMock.Setup(r => r.GetSlotAsync(It.IsAny<SlotModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServerSlot?)null);

            // Act & Assert
            var result = await controller.UpdateSlot(request, CancellationToken.None);

            // Assert
            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());

            repositoryMock.Verify(r => r.GetSlotAsync(It.IsAny<SlotModel>(), It.IsAny<CancellationToken>()), Times.Once);
            repositoryMock.Verify(r => r.UpdateSlotAsync(It.IsAny<ServerSlot>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UpdateSlot_ValidRequest_UpdatesSlotReturnsOk()
        {
            // Arrange
            var email = "user@example.com";
            var request = new UpdateSlotRequest
            {
                Id = "123",
                Name = "Updated Slot Name"
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var existingSlot = new ServerSlot
            {
                Id = "123",
                UserEmail = email,
                Name = "Old Slot Name"
            };
            var updatedSlot = new ServerSlot
            {
                Id = request.Id,
                UserEmail = email,
                Name = request.Name
            };

            repositoryMock.Setup(r => r.GetSlotAsync(It.IsAny<SlotModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSlot);

            mapperMock.Setup(m => m.Map<ServerSlot>(request))
                .Returns(updatedSlot);

            repositoryMock.Setup(r => r.UpdateSlotAsync(It.IsAny<ServerSlot>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.UpdateSlot(request, CancellationToken.None);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());

            repositoryMock.Verify(r => r.GetSlotAsync(It.IsAny<SlotModel>(), It.IsAny<CancellationToken>()), Times.Once);
            repositoryMock.Verify(r => r.UpdateSlotAsync(It.IsAny<ServerSlot>(), It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.Verify(m => m.Map<ServerSlot>(request), Times.Once);
        }
    }
}