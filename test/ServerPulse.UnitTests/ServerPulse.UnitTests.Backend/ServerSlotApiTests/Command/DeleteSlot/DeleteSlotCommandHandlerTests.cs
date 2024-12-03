using Microsoft.Extensions.Configuration;
using Moq;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;
using Shared.Helpers;

namespace ServerSlotApi.Command.DeleteSlot.Tests
{
    [TestFixture]
    internal class DeleteSlotCommandHandlerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IHttpHelper> httpHelperMock;
        private Mock<IConfiguration> configurationMock;
        private DeleteSlotCommandHandler handler;
        private const string ApiGateway = "http://api.example.com/";
        private const string DeleteStatisticsEndpoint = "statistics/delete/";

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            httpHelperMock = new Mock<IHttpHelper>();
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c[Configuration.API_GATEWAY]).Returns(ApiGateway);
            configurationMock.Setup(c => c[Configuration.STATISTICS_DELETE_URL]).Returns(DeleteStatisticsEndpoint);

            handler = new DeleteSlotCommandHandler(
                repositoryMock.Object,
                httpHelperMock.Object,
                configurationMock.Object
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

            httpHelperMock
                .Setup(h => h.SendDeleteRequestAsync(It.Is<string>(url => url == $"{ApiGateway}{DeleteStatisticsEndpoint}{slot.SlotKey}"), token, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            repositoryMock.Verify(r => r.DeleteSlotAsync(slot, It.IsAny<CancellationToken>()), Times.Once);
            httpHelperMock.Verify(h => h.SendDeleteRequestAsync(It.IsAny<string>(), token, It.IsAny<CancellationToken>()), Times.Once);
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
            httpHelperMock.Verify(h => h.SendDeleteRequestAsync(It.IsAny<string>(), token, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}