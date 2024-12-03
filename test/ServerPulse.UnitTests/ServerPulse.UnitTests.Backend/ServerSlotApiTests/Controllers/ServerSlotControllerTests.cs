using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Command.CheckSlotKey;
using ServerSlotApi.Command.CreateSlot;
using ServerSlotApi.Command.DeleteSlot;
using ServerSlotApi.Command.GetSlotById;
using ServerSlotApi.Command.GetSlotsByEmail;
using ServerSlotApi.Command.UpdateSlot;
using ServerSlotApi.Controllers;
using ServerSlotApi.Dtos;
using System.Security.Claims;


namespace ServerSlotApiTests.Controllers
{
    [TestFixture]
    internal class ServerSlotControllerTests
    {
        private Mock<IMediator> mediatorMock;
        private ServerSlotController controller;

        [SetUp]
        public void SetUp()
        {
            mediatorMock = new Mock<IMediator>();

            controller = new ServerSlotController(mediatorMock.Object);
        }

        private static void SetUserContext(string email, ServerSlotController controller, bool setAuthorization = false)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
               new Claim(ClaimTypes.Email, email),
            ], "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            if (setAuthorization)
            {
                controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer valid-token";
            }
        }

        private static IEnumerable<TestCaseData> GetSlotsByEmailTestCases()
        {
            yield return new TestCaseData("test@example.com", "", new List<ServerSlotResponse>
            {
                new ServerSlotResponse { Id = "1", UserEmail = "test@example.com", Name = "Slot1" }
            }).SetDescription("Returns slots matching user email and empty query.");

            yield return new TestCaseData("test@example.com", "SearchQuery", new List<ServerSlotResponse>())
                .SetDescription("No slots match the query for user email.");
        }

        [Test]
        [TestCaseSource(nameof(GetSlotsByEmailTestCases))]
        public async Task GetSlotsByEmail_TestCases(string email, string searchString, IEnumerable<ServerSlotResponse> expectedResponse)
        {
            // Arrange
            SetUserContext(email, controller);

            mediatorMock.Setup(m => m.Send(It.IsAny<GetSlotsByEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetSlotsByEmail(searchString, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
        }

        private static IEnumerable<TestCaseData> GetSlotByIdTestCases()
        {
            yield return new TestCaseData("test@example.com", "1", new ServerSlotResponse
            {
                Id = "1",
                UserEmail = "test@example.com",
                Name = "Slot1"
            }).SetDescription("Slot with given ID exists.");

            yield return new TestCaseData("test@example.com", "non-existent-id", null)
                .SetDescription("Slot with given ID does not exist.");
        }

        [Test]
        [TestCaseSource(nameof(GetSlotByIdTestCases))]
        public async Task GetSlotById_TestCases(string email, string slotId, ServerSlotResponse? expectedResponse)
        {
            // Arrange
            SetUserContext(email, controller);

            mediatorMock.Setup(m => m.Send(It.IsAny<GetSlotByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse!);

            // Act
            var result = await controller.GetSlotById(slotId, CancellationToken.None);

            // Assert
            if (expectedResponse != null)
            {
                Assert.NotNull(result);
                Assert.NotNull(result.Result);

                Assert.IsInstanceOf<OkObjectResult>(result.Result);
                var okResult = result.Result as OkObjectResult;
                Assert.IsNotNull(okResult);

                Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
            }
            else
            {
                Assert.NotNull(result);
                Assert.NotNull(result.Result);

                Assert.IsInstanceOf<OkObjectResult>(result.Result);
                var okResult = result.Result as OkObjectResult;
                Assert.IsNotNull(okResult);

                Assert.IsNull(okResult.Value);
            }
        }

        [Test]
        public async Task CheckSlotKey_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = "valid-key" };
            var expectedResponse = new CheckSlotKeyResponse { IsExisting = true };

            mediatorMock.Setup(m => m.Send(It.IsAny<CheckSlotKeyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CheckSlotKey(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Result);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
        }

        [Test]
        public async Task CreateSlot_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var email = "test@example.com";
            var request = new CreateServerSlotRequest { Name = "NewSlot" };
            var expectedResponse = new ServerSlotResponse { Id = "1", UserEmail = email, Name = "NewSlot" };

            SetUserContext(email, controller);

            mediatorMock.Setup(m => m.Send(It.IsAny<CreateSlotCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CreateSlot(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<CreatedResult>(result.Result);
            var createdResult = result.Result as CreatedResult;
            Assert.IsNotNull(createdResult);

            Assert.That(createdResult.Value, Is.EqualTo(expectedResponse));
        }

        [Test]
        public async Task UpdateSlot_ValidRequest_ReturnsOk()
        {
            // Arrange
            var email = "test@example.com";
            var request = new UpdateServerSlotRequest { Id = "1", Name = "UpdatedSlot" };

            SetUserContext(email, controller);

            mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSlotCommand>(), It.IsAny<CancellationToken>()));

            // Act
            var result = await controller.UpdateSlot(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task DeleteSlot_ValidRequest_ReturnsOk()
        {
            // Arrange
            var email = "test@example.com";
            var slotId = "1";

            SetUserContext(email, controller, true);

            mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSlotCommand>(), It.IsAny<CancellationToken>()));

            // Act
            var result = await controller.DeleteSlot(slotId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            Assert.IsInstanceOf<OkResult>(result);
        }
    }
}