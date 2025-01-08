using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Dtos.Endpoints.ServerSlot.CreateSlot;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Repositories;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.CreateSlot.Tests
{
    [TestFixture]
    internal class CreateSlotControllerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private CreateSlotController controller;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();

            controller = new CreateSlotController(repositoryMock.Object, mapperMock.Object);

            controller.Url = new Mock<IUrlHelper>().Object;
        }

        [TestCase("", "TestSlot", ExpectedResult = typeof(ConflictObjectResult))]
        [TestCase("user@example.com", "TestSlot", ExpectedResult = typeof(CreatedResult))]
        public async Task<Type?> CreateSlot_EmailValidation_ReturnsExpectedResult(string? email, string slotName)
        {
            // Arrange
            var request = new CreateSlotRequest { Name = slotName };
            var serverSlot = new ServerSlot { Name = slotName, UserEmail = email ?? "" };
            var expectedResponse = new CreateSlotResponse { Id = "123", Name = slotName, UserEmail = email };
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            mapperMock
                .Setup(m => m.Map<ServerSlot>(request))
                .Returns(serverSlot);

            mapperMock
                .Setup(m => m.Map<CreateSlotResponse>(It.IsAny<ServerSlot>()))
                .Returns(expectedResponse);

            // Act 
            var result = await controller.CreateSlot(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result.Result);

            return result.Result.GetType();
        }

        [Test]
        [TestCase("user@example.com", "ValidSlot")]
        public async Task CreateSlot_ValidCommand_CreatesSlotAndReturnsResponse(string email, string slotName)
        {
            // Arrange
            var request = new CreateSlotRequest { Name = slotName };
            var serverSlot = new ServerSlot { Name = slotName, UserEmail = email };
            var expectedResponse = new CreateSlotResponse { Id = "123", Name = slotName, UserEmail = email };
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Email, email!),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            mapperMock
                .Setup(m => m.Map<ServerSlot>(request))
                .Returns(serverSlot);

            repositoryMock
                .Setup(r => r.CreateSlotAsync(It.IsAny<ServerSlot>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serverSlot);

            mapperMock
                .Setup(m => m.Map<CreateSlotResponse>(serverSlot))
                .Returns(expectedResponse);

            // Act
            var result = await controller.CreateSlot(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedResult>());
            var response = (result.Result as CreatedResult)?.Value as CreateSlotResponse;
            Assert.IsNotNull(response);

            Assert.That(response.Id, Is.EqualTo("123"));
            Assert.That(response.Name, Is.EqualTo(expectedResponse.Name));
            Assert.That(response.UserEmail, Is.EqualTo(expectedResponse.UserEmail));

            repositoryMock.Verify(r => r.CreateSlotAsync(serverSlot, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.Verify(m => m.Map<ServerSlot>(request), Times.Once);
            mapperMock.Verify(m => m.Map<CreateSlotResponse>(serverSlot), Times.Once);
        }
    }
}