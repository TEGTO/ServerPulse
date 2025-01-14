using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Core.Entities;
using ServerSlotApi.Core.Models;
using ServerSlotApi.Infrastructure.Repositories;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.DeleteSlot.Tests
{
    [TestFixture]
    internal class DeleteSlotControllerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private DeleteSlotController controller;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();

            controller = new DeleteSlotController(
                repositoryMock.Object
            );
        }

        [TestCase("", "slot123", ExpectedResult = typeof(ConflictObjectResult))]
        [TestCase("user@example.com", "slot123", ExpectedResult = typeof(OkResult))]
        public async Task<Type?> DeleteSlot_EmailValidation_ReturnsExpectedResult(string? email, string slotId)
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = await controller.DeleteSlot(slotId, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);

            return result.GetType();
        }

        [Test]
        public async Task DeleteSlot_SlotExists_DeletesSlotAndStatistics()
        {
            // Arrange
            var email = "user@example.com";
            var slotId = "slot123";
            var slot = new ServerSlot { UserEmail = email };

            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
               new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            repositoryMock
                .Setup(r => r.GetSlotAsync(It.Is<GetSlot>(m => m.SlotId == slotId && m.UserEmail == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            repositoryMock
                .Setup(r => r.DeleteSlotAsync(slot, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await controller.DeleteSlot(slotId, CancellationToken.None);

            // Assert
            repositoryMock.Verify(r => r.DeleteSlotAsync(slot, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteSlot_SlotDoesNotExist_NoDeletion()
        {
            // Arrange
            var email = "user@example.com";
            var slotId = "slot123";

            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
               new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            repositoryMock
                .Setup(r => r.GetSlotAsync(It.Is<GetSlot>(m => m.SlotId == slotId && m.UserEmail == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServerSlot?)null);

            // Act
            await controller.DeleteSlot(slotId, CancellationToken.None);

            // Assert
            repositoryMock.Verify(r => r.DeleteSlotAsync(It.IsAny<ServerSlot>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}