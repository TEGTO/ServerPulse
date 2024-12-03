using AutoMapper;
using MediatR;
using Moq;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.UpdateSlot.Tests
{
    [TestFixture]
    internal class UpdateSlotCommandHandlerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private UpdateSlotCommandHandler handler;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();
            handler = new UpdateSlotCommandHandler(repositoryMock.Object, mapperMock.Object);
        }

        [TestCase(null, ExpectedResult = typeof(ArgumentNullException))]
        [TestCase("", ExpectedResult = typeof(ArgumentException))]
        [TestCase("user@example.com", ExpectedResult = null)]
        public async Task<Type?> Handle_EmailValidation_ThrowsException(string? email)
        {
            // Arrange
            var updateRequest = new UpdateServerSlotRequest
            {
                Id = "123",
                Name = "Updated Slot Name"
            };

            var command = new UpdateSlotCommand(email, updateRequest);

            var mappedSlot = new ServerSlot
            {
                Id = updateRequest.Id,
                UserEmail = email!,
                Name = updateRequest.Name
            };

            mapperMock
                .Setup(m => m.Map<ServerSlot>(updateRequest))
                .Returns(mappedSlot);

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
        public async Task Handle_ValidRequest_UpdatesSlot()
        {
            // Arrange
            var email = "user@example.com";
            var updateRequest = new UpdateServerSlotRequest
            {
                Id = "123",
                Name = "Updated Slot Name"
            };
            var command = new UpdateSlotCommand(email, updateRequest);

            var mappedSlot = new ServerSlot
            {
                Id = updateRequest.Id,
                UserEmail = email,
                Name = updateRequest.Name
            };

            mapperMock
                .Setup(m => m.Map<ServerSlot>(updateRequest))
                .Returns(mappedSlot);

            repositoryMock
                .Setup(r => r.UpdateSlotAsync(mappedSlot, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(Unit.Value));

            mapperMock.Verify(m => m.Map<ServerSlot>(updateRequest), Times.Once);
            repositoryMock.Verify(r => r.UpdateSlotAsync(mappedSlot, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}