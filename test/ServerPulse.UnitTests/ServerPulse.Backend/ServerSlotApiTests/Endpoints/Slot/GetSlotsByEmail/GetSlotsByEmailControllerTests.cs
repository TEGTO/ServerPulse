using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Dtos.Endpoints.ServerSlot.GetSlotsByEmail;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Repositories;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.GetSlotsByEmail.Tests
{
    [TestFixture]
    internal class GetSlotsByEmailControllerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private GetSlotsByEmailController controller;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();

            controller = new GetSlotsByEmailController(repositoryMock.Object, mapperMock.Object);
        }

        [TestCase("", ExpectedResult = typeof(ConflictObjectResult))]
        [TestCase("user@example.com", ExpectedResult = typeof(OkObjectResult))]
        public async Task<Type?> GetSlotsByEmail_EmailValidation_ReturnsExpectedResult(string? email)
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
            var result = await controller.GetSlotsByEmail();

            // Assert
            Assert.IsNotNull(result.Result);

            return result.Result.GetType();
        }

        [TestCase("user@example.com", "", 2, Description = "Valid email and slots exist")]
        [TestCase("user@example.com", "", 0, Description = "Valid email and no slots exist")]
        [TestCase("user@example.com", "Test", 1, Description = "Valid email with filter string")]
        public async Task GetSlotsByEmail_SlotsHandling_ReturnsExpectedResult(string email, string containsString, int expectedSlotCount)
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

            var slots = Enumerable.Range(1, expectedSlotCount).Select(i => new ServerSlot
            {
                Id = $"slot-{i}",
                UserEmail = email,
                Name = $"Test Slot {i}"
            }).ToList();

            var expectedResponses = slots.Select(slot => new GetSlotsByEmailResponse
            {
                Id = slot.Id,
                UserEmail = slot.UserEmail,
                Name = slot.Name,
                SlotKey = "key-" + slot.Id
            }).ToList();

            repositoryMock
                .Setup(r => r.GetSlotsByUserEmailAsync(email, containsString, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slots);

            foreach (var slot in slots)
            {
                mapperMock
                    .Setup(m => m.Map<GetSlotsByEmailResponse>(slot))
                    .Returns(expectedResponses.First(r => r.Id == slot.Id));
            }

            // Act
            var result = await controller.GetSlotsByEmail(containsString);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as IEnumerable<GetSlotsByEmailResponse>;
            Assert.IsNotNull(response);

            Assert.That(response.Count, Is.EqualTo(expectedSlotCount));

            repositoryMock.Verify(r =>
                r.GetSlotsByUserEmailAsync(email, containsString, It.IsAny<CancellationToken>()), Times.Once);

            foreach (var slot in slots)
            {
                mapperMock.Verify(m => m.Map<GetSlotsByEmailResponse>(slot), Times.Once);
            }
        }
    }
}