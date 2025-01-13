using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.GetSlotById;
using ServerSlotApi.Core.Entities;
using ServerSlotApi.Core.Models;
using ServerSlotApi.Infrastructure.Repositories;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.GetSlotById.Tests
{
    [TestFixture]
    internal class GetSlotByIdControllerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private GetSlotByIdController controller;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();

            controller = new GetSlotByIdController(repositoryMock.Object, mapperMock.Object);
        }

        [TestCase("", "slot123", ExpectedResult = typeof(ConflictObjectResult))]
        [TestCase("user@example.com", "slot123", ExpectedResult = typeof(NotFoundResult))]
        public async Task<Type?> GetSlotById_EmailValidation_ReturnsExpectedResult(string? email, string slotId)
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
            var result = await controller.GetSlotById(slotId, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Result);

            return result.Result.GetType();
        }

        [TestCase("user@example.com", "slot123", true, Description = "Valid email and slot exists")]
        [TestCase("user@example.com", "slot123", false, Description = "Valid email but slot does not exist")]
        public async Task GetSlotById_SlotHandling_ReturnsExpectedResult(string email, string slotId, bool slotExists)
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

            var slot = slotExists ? new ServerSlot
            {
                Id = slotId,
                UserEmail = email,
                Name = "Test Slot"
            } : null;

            var expectedResponse = slotExists ? new GetSlotByIdResponse
            {
                Id = slotId,
                UserEmail = email,
                Name = "Test Slot",
                SlotKey = "slot-key"
            } : null;

            repositoryMock
                .Setup(r => r.GetSlotAsync(It.Is<GetSlot>(m => m.SlotId == slotId && m.UserEmail == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            mapperMock
                .Setup(m => m.Map<GetSlotByIdResponse>(slot))
                .Returns(expectedResponse!);

            // Act
            var result = await controller.GetSlotById(slotId, CancellationToken.None);

            // Assert
            if (slotExists)
            {
                Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
                var response = (result.Result as OkObjectResult)?.Value as GetSlotByIdResponse;
                Assert.NotNull(response);

                Assert.That(response.Id, Is.EqualTo(expectedResponse!.Id));
                Assert.That(response.Name, Is.EqualTo(expectedResponse!.Name));
                Assert.That(response.UserEmail, Is.EqualTo(expectedResponse!.UserEmail));

                repositoryMock.Verify(r => r.GetSlotAsync(It.IsAny<GetSlot>(), It.IsAny<CancellationToken>()), Times.Once);
                mapperMock.Verify(m => m.Map<GetSlotByIdResponse>(slot), Times.Once);
            }
            else
            {
                Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
            }
        }
    }
}