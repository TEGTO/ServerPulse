using Moq;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.DeleteSlot.Tests
{
    [TestFixture]
    internal class DeleteSlotCommandHandlerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private DeleteSlotCommandHandler handler;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();

            handler = new DeleteSlotCommandHandler(
                repositoryMock.Object
            );
        }

        [TestCase(null, "slot123", "validToken", ExpectedResult = typeof(ArgumentNullException))]
        [TestCase("", "slot123", "validToken", ExpectedResult = typeof(ArgumentException))]
        [TestCase("user@example.com", "slot123", "validToken", ExpectedResult = null)]
        public async Task<Type?> Handle_EmailValidation_ThrowsException(string? email, string slotId, string token)
        {
            // Arrange
            var command = new DeleteSlotCommand(email, slotId, token);

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

        [Test]
        public async Task Handle_SlotExists_DeletesSlotAndStatistics()
        {
            // Arrange
            var email = "user@example.com";
            var slotId = "slot123";
            var token = "validToken";
            var slot = new ServerSlot { UserEmail = email };

            var command = new DeleteSlotCommand(email, slotId, token);

            repositoryMock
                .Setup(r => r.GetSlotAsync(It.Is<SlotModel>(m => m.SlotId == slotId && m.UserEmail == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            repositoryMock
                .Setup(r => r.DeleteSlotAsync(slot, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            repositoryMock.Verify(r => r.DeleteSlotAsync(slot, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_SlotDoesNotExist_NoDeletionOrStatisticsRequest()
        {
            // Arrange
            var email = "user@example.com";
            var slotId = "slot123";
            var token = "validToken";

            var command = new DeleteSlotCommand(email, slotId, token);

            repositoryMock
                .Setup(r => r.GetSlotAsync(It.Is<SlotModel>(m => m.SlotId == slotId && m.UserEmail == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServerSlot?)null);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            repositoryMock.Verify(r => r.DeleteSlotAsync(It.IsAny<ServerSlot>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}