using AutoMapper;
using Moq;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.CreateSlot.Tests
{
    [TestFixture]
    internal class CreateSlotCommandHandlerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private CreateSlotCommandHandler handler;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();
            handler = new CreateSlotCommandHandler(repositoryMock.Object, mapperMock.Object);
        }

        [TestCase(null, "TestSlot", ExpectedResult = typeof(ArgumentNullException))]
        [TestCase("", "TestSlot", ExpectedResult = typeof(ArgumentException))]
        [TestCase("user@example.com", "TestSlot", ExpectedResult = null)]
        public async Task<Type?> Handle_EmailValidation_ReturnsExpectedException(string? email, string slotName)
        {
            // Arrange
            var command = new CreateSlotCommand(email, new CreateServerSlotRequest { Name = slotName });
            var serverSlot = new ServerSlot { Name = slotName, UserEmail = email ?? "" };

            mapperMock
                .Setup(m => m.Map<ServerSlot>(command.Request))
                .Returns(serverSlot);

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
        [TestCase("user@example.com", "ValidSlot")]
        public async Task Handle_ValidCommand_CreatesSlotAndReturnsResponse(string email, string slotName)
        {
            // Arrange
            var command = new CreateSlotCommand(email, new CreateServerSlotRequest { Name = slotName });
            var serverSlot = new ServerSlot { Name = slotName, UserEmail = email };
            var serverSlotResponse = new ServerSlotResponse { Id = "123", Name = slotName, UserEmail = email };

            mapperMock
                .Setup(m => m.Map<ServerSlot>(command.Request))
                .Returns(serverSlot);

            repositoryMock
                .Setup(r => r.CreateSlotAsync(It.IsAny<ServerSlot>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serverSlot);

            mapperMock
                .Setup(m => m.Map<ServerSlotResponse>(serverSlot))
                .Returns(serverSlotResponse);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo("123"));
            Assert.That(result.Name, Is.EqualTo(serverSlotResponse.Name));
            Assert.That(result.UserEmail, Is.EqualTo(serverSlotResponse.UserEmail));

            repositoryMock.Verify(r => r.CreateSlotAsync(serverSlot, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.Verify(m => m.Map<ServerSlot>(command.Request), Times.Once);
            mapperMock.Verify(m => m.Map<ServerSlotResponse>(serverSlot), Times.Once);
        }
    }
}