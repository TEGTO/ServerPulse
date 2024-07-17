using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Controllers;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Domain.Entities;
using ServerSlotApi.Dtos;
using ServerSlotApi.Services;
using System.Security.Claims;

namespace ServerSlotApiTests.Controllers
{
    [TestFixture]
    internal class ServerSlotControllerTests
    {
        private Mock<IMapper> mapperMock;
        private Mock<IServerSlotService> serverSlotServiceMock;
        private ServerSlotController serverSlotController;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            mapperMock = new Mock<IMapper>();
            serverSlotServiceMock = new Mock<IServerSlotService>();
            serverSlotController = new ServerSlotController(mapperMock.Object, serverSlotServiceMock.Object);
            cancellationToken = new CancellationToken();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
            }, "mock"));

            serverSlotController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetServerSlotsByEmail_ValidEmail_ReturnsServerSlots()
        {
            // Arrange
            var email = "test@example.com";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { UserEmail = email, Name = "Slot1" },
                new ServerSlot { UserEmail = email, Name = "Slot2" }
            };
            var serverSlotResponses = new List<ServerSlotResponse>
            {
                new ServerSlotResponse { Name = "Slot1" },
                new ServerSlotResponse { Name = "Slot2" }
            };
            serverSlotServiceMock.Setup(s => s.GetServerSlotsByEmailAsync(email, cancellationToken)).ReturnsAsync(serverSlots);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(It.IsAny<ServerSlot>())).Returns((ServerSlot src) => new ServerSlotResponse { Name = src.Name });
            // Act
            var result = await serverSlotController.GetServerSlotsByEmail(cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as IEnumerable<ServerSlotResponse>;
            Assert.That(response.Count(), Is.EqualTo(2));
            Assert.That(response.First().Name, Is.EqualTo("Slot1"));
        }
        [Test]
        public async Task GerServerSlotsContainingString_ValidEmailAndString_ReturnsServerSlots()
        {
            // Arrange
            var email = "test@example.com";
            var searchString = "Slot";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { UserEmail = email, Name = "Slot1" },
                new ServerSlot { UserEmail = email, Name = "Slot2" }
            };
            serverSlotServiceMock.Setup(s => s.GerServerSlotsContainingStringAsync(email, searchString, cancellationToken)).ReturnsAsync(serverSlots);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(It.IsAny<ServerSlot>())).Returns((ServerSlot src) => new ServerSlotResponse { Name = src.Name });
            // Act
            var result = await serverSlotController.GerServerSlotsContainingString(searchString, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as IEnumerable<ServerSlotResponse>;
            Assert.That(response.Count(), Is.EqualTo(2));
            Assert.IsTrue(response.All(x => x.Name.Contains(searchString)));
        }
        [Test]
        public async Task CreateServerSlot_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreateServerSlotRequest { Name = "NewSlot" };
            var serverSlot = new ServerSlot { UserEmail = "test@example.com", Name = "NewSlot" };
            var serverSlotResponse = new ServerSlotResponse { Name = "NewSlot" };
            serverSlotServiceMock.Setup(s => s.CreateServerSlotAsync(It.IsAny<ServerSlot>(), cancellationToken)).ReturnsAsync(serverSlot);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(serverSlot)).Returns(serverSlotResponse);
            // Act
            var result = await serverSlotController.CreateServerSlot(request, cancellationToken);
            // Assert
            Assert.IsInstanceOf<CreatedResult>(result.Result);
            var createdResult = result.Result as CreatedResult;
            var response = createdResult.Value as ServerSlotResponse;
            Assert.That(response, Is.EqualTo(serverSlotResponse));
        }
        [Test]
        public async Task UpdateServerSlot_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new UpdateServerSlotRequest { Id = "1", Name = "UpdatedSlot" };
            var serverSlot = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "UpdatedSlot" };
            mapperMock.Setup(m => m.Map<ServerSlot>(request)).Returns(serverSlot);
            serverSlotServiceMock.Setup(s => s.UpdateServerSlotAsync(serverSlot, cancellationToken)).Returns(Task.CompletedTask);
            // Act
            var result = await serverSlotController.UpdateServerSlot(request, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task DeleteServerSlot_ValidId_ReturnsOk()
        {
            // Arrange
            var id = "1";
            serverSlotServiceMock.Setup(s => s.DeleteServerSlotByIdAsync(id, cancellationToken)).Returns(Task.CompletedTask);
            // Act
            var result = await serverSlotController.DeleteServerSlot(id, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
    }
}