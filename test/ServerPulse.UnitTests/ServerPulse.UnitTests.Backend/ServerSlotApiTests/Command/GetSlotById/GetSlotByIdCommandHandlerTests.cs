using AutoMapper;
using Moq;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.GetSlotById.Tests
{
    [TestFixture]
    internal class GetSlotByIdCommandHandlerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private GetSlotByIdCommandHandler handler;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();
            handler = new GetSlotByIdCommandHandler(repositoryMock.Object, mapperMock.Object);
        }

        [TestCase(null, "slot123", ExpectedResult = typeof(ArgumentNullException))]
        [TestCase("", "slot123", ExpectedResult = typeof(ArgumentException))]
        [TestCase("user@example.com", "slot123", ExpectedResult = null)]
        public async Task<Type?> Handle_EmailValidation_ThrowsException(string? email, string slotId)
        {
            // Arrange
            var command = new GetSlotByIdCommand(email, slotId);

            if (email == null)
            {
                // Act & Assert
                Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await handler.Handle(command, CancellationToken.None));
                return typeof(ArgumentNullException);
            }
            else if (string.IsNullOrEmpty(email))
            {
                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(async () =>
                    await handler.Handle(command, CancellationToken.None));
                return typeof(ArgumentException);
            }

            // Act
            await handler.Handle(command, CancellationToken.None);
            return null;
        }

        [TestCase("user@example.com", "slot123", true, Description = "Valid email and slot exists")]
        [TestCase("user@example.com", "slot123", false, Description = "Valid email but slot does not exist")]
        public async Task Handle_SlotHandling_ReturnsExpectedResult(string email, string slotId, bool slotExists)
        {
            // Arrange
            var command = new GetSlotByIdCommand(email, slotId);
            var slot = slotExists ? new ServerSlot
            {
                Id = slotId,
                UserEmail = email,
                Name = "Test Slot"
            } : null;

            var expectedResponse = slotExists ? new ServerSlotResponse
            {
                Id = slotId,
                UserEmail = email,
                Name = "Test Slot",
                SlotKey = "slot-key"
            } : null;

            repositoryMock
                .Setup(r => r.GetSlotAsync(It.Is<SlotModel>(m => m.SlotId == slotId && m.UserEmail == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            mapperMock
                .Setup(m => m.Map<ServerSlotResponse>(slot))
                .Returns(expectedResponse!);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            if (slotExists)
            {
                Assert.IsNotNull(result);
                Assert.That(result.Id, Is.EqualTo(expectedResponse!.Id));
                Assert.That(result.Name, Is.EqualTo(expectedResponse!.Name));
                Assert.That(result.UserEmail, Is.EqualTo(expectedResponse!.UserEmail));
            }
            else
            {
                Assert.IsNull(result);
            }

            repositoryMock.Verify(r => r.GetSlotAsync(It.IsAny<SlotModel>(), It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.Verify(m => m.Map<ServerSlotResponse>(slot), Times.Once);
        }
    }
}