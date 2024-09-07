using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Controllers;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Domain.Entities;
using ServerSlotApi.Dtos;
using ServerSlotApi.Services;
using Shared.Dtos.ServerSlot;
using System.Security.Claims;

namespace ServerSlotApiTests.Controllers
{
    [TestFixture]
    internal class ServerSlotControllerTests
    {
        private Mock<IMapper> mapperMock;
        private Mock<IServerSlotService> serverSlotServiceMock;
        private Mock<ISlotStatisticsService> slotStatisticsServiceMock;
        private ServerSlotController serverSlotController;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            mapperMock = new Mock<IMapper>();
            serverSlotServiceMock = new Mock<IServerSlotService>();
            slotStatisticsServiceMock = new Mock<ISlotStatisticsService>();
            serverSlotController = new ServerSlotController(mapperMock.Object, serverSlotServiceMock.Object, slotStatisticsServiceMock.Object);
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
            serverSlotServiceMock.Setup(s => s.GetSlotsByEmailAsync(email, cancellationToken)).ReturnsAsync(serverSlots);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(It.IsAny<ServerSlot>())).Returns((ServerSlot src) => new ServerSlotResponse { Name = src.Name });
            // Act
            var result = await serverSlotController.GetSlotsByEmail(cancellationToken);
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
            serverSlotServiceMock.Setup(s => s.GetSlotsContainingStringAsync(email, searchString, cancellationToken)).ReturnsAsync(serverSlots);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(It.IsAny<ServerSlot>())).Returns((ServerSlot src) => new ServerSlotResponse { Name = src.Name });
            // Act
            var result = await serverSlotController.GetSlotsContainingString(searchString, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as IEnumerable<ServerSlotResponse>;
            Assert.That(response.Count(), Is.EqualTo(2));
            Assert.IsTrue(response.All(x => x.Name.Contains(searchString)));
        }
        [Test]
        public async Task CheckSlotKey_ValidRequest_ReturnsCorrectResponse()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = "valid-key" };
            serverSlotServiceMock.Setup(s => s.CheckIfKeyValidAsync(request.SlotKey, cancellationToken)).ReturnsAsync(true);
            // Act
            var result = await serverSlotController.CheckSlotKey(request, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as CheckSlotKeyResponse;
            Assert.NotNull(response);
            Assert.That(response.SlotKey, Is.EqualTo(request.SlotKey));
            Assert.IsTrue(response.IsExisting);
        }
        [Test]
        public async Task CheckSlotKey_InvalidKey_ReturnsCorrectResponse()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = "invalid-key" };
            serverSlotServiceMock.Setup(s => s.CheckIfKeyValidAsync(request.SlotKey, cancellationToken)).ReturnsAsync(false);
            // Act
            var result = await serverSlotController.CheckSlotKey(request, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as CheckSlotKeyResponse;
            Assert.NotNull(response);
            Assert.That(response.SlotKey, Is.EqualTo(request.SlotKey));
            Assert.IsFalse(response.IsExisting);
        }
        [Test]
        public async Task CreateServerSlot_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreateServerSlotRequest { Name = "NewSlot" };
            var serverSlot = new ServerSlot { UserEmail = "test@example.com", Name = "NewSlot" };
            var serverSlotResponse = new ServerSlotResponse { Name = "NewSlot" };
            serverSlotServiceMock.Setup(s => s.CreateSlotAsync(It.IsAny<ServerSlot>(), cancellationToken)).ReturnsAsync(serverSlot);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(serverSlot)).Returns(serverSlotResponse);
            // Act
            var result = await serverSlotController.CreateSlot(request, cancellationToken);
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
            serverSlotServiceMock.Setup(s => s.UpdateSlotAsync(It.IsAny<SlotParams>(), serverSlot, cancellationToken)).Returns(Task.CompletedTask);
            // Act
            var result = await serverSlotController.UpdateSlot(request, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task DeleteServerSlot_ValidId_ReturnsOk()
        {
            // Arrange
            var id = "1";
            serverSlotServiceMock.Setup(s => s.DeleteSlotByIdAsync(It.IsAny<SlotParams>(), cancellationToken)).Returns(Task.CompletedTask);
            // Act
            var result = await serverSlotController.DeleteSlot(id, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task GetSlotById_ValidId_ReturnsSlot()
        {
            // Arrange
            var id = "1";
            var serverSlot = new ServerSlot { Id = id, UserEmail = "test@example.com", Name = "Slot1" };
            var serverSlotResponse = new ServerSlotResponse { Name = "Slot1" };
            serverSlotServiceMock.Setup(s => s.GetSlotByIdAsync(It.IsAny<SlotParams>(), cancellationToken)).ReturnsAsync(serverSlot);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(serverSlot)).Returns(serverSlotResponse);
            // Act
            var result = await serverSlotController.GetSlotById(id, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(serverSlotResponse));
        }
        [Test]
        public async Task GetSlotById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = "invalid-id";
            serverSlotServiceMock.Setup(s => s.GetSlotByIdAsync(It.IsAny<SlotParams>(), cancellationToken)).ReturnsAsync((ServerSlot)null);
            // Act
            var result = await serverSlotController.GetSlotById(id, cancellationToken);
            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }
        [Test]
        public async Task GerSlotsContainingString_ValidString_ReturnsServerSlots()
        {
            // Arrange
            var email = "test@example.com";
            var searchString = "Slot";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { UserEmail = email, Name = "Slot1" },
                new ServerSlot { UserEmail = email, Name = "Slot2" }
            };
            serverSlotServiceMock.Setup(s => s.GetSlotsContainingStringAsync(email, searchString, cancellationToken)).ReturnsAsync(serverSlots);
            mapperMock.Setup(m => m.Map<ServerSlotResponse>(It.IsAny<ServerSlot>())).Returns((ServerSlot src) => new ServerSlotResponse { Name = src.Name });
            // Act
            var result = await serverSlotController.GetSlotsContainingString(searchString, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as IEnumerable<ServerSlotResponse>;
            Assert.That(response.Count(), Is.EqualTo(2));
            Assert.IsTrue(response.All(x => x.Name.Contains(searchString)));
        }
        [Test]
        public async Task DeleteSlot_ValidIdAndStatisticsDeleted_ReturnsOk()
        {
            // Arrange
            var id = "1";
            var serverSlot = new ServerSlot { Id = id };
            var email = "test@example.com";
            var token = "valid-token";
            serverSlotServiceMock.Setup(s => s.GetSlotByIdAsync(It.IsAny<SlotParams>(), cancellationToken)).ReturnsAsync(serverSlot);
            slotStatisticsServiceMock.Setup(s => s.DeleteSlotStatisticsAsync(serverSlot.SlotKey, token, cancellationToken)).ReturnsAsync(true);
            serverSlotServiceMock.Setup(s => s.DeleteSlotByIdAsync(It.IsAny<SlotParams>(), cancellationToken)).Returns(Task.CompletedTask);
            serverSlotController.Request.Headers["Authorization"] = $"Bearer {token}";
            // Act
            var result = await serverSlotController.DeleteSlot(id, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
            serverSlotServiceMock.Verify(s => s.DeleteSlotByIdAsync(It.IsAny<SlotParams>(), cancellationToken), Times.Once);
            slotStatisticsServiceMock.Verify(s => s.DeleteSlotStatisticsAsync(serverSlot.SlotKey, token, cancellationToken), Times.Once);
        }
        [Test]
        public async Task DeleteSlot_StatisticsDeletionFails_ReturnsInternalServerError()
        {
            // Arrange
            var id = "1";
            var serverSlot = new ServerSlot { Id = id };
            var email = "test@example.com";
            var token = "valid-token";
            serverSlotServiceMock.Setup(s => s.GetSlotByIdAsync(It.IsAny<SlotParams>(), cancellationToken)).ReturnsAsync(serverSlot);
            slotStatisticsServiceMock.Setup(s => s.DeleteSlotStatisticsAsync(serverSlot.SlotKey, token, cancellationToken)).ReturnsAsync(false);
            serverSlotController.Request.Headers["Authorization"] = $"Bearer {token}";
            // Act
            var result = await serverSlotController.DeleteSlot(id, cancellationToken);
            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value, Is.EqualTo("Failed to delete server slot!"));
            serverSlotServiceMock.Verify(s => s.DeleteSlotByIdAsync(It.IsAny<SlotParams>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}