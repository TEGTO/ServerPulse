using AutoMapper;
using Moq;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.GetSlotsByEmail.Tests
{
    [TestFixture]
    internal class GetSlotsByEmailCommandHandlerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private Mock<IMapper> mapperMock;
        private GetSlotsByEmailCommandHandler handler;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            mapperMock = new Mock<IMapper>();
            handler = new GetSlotsByEmailCommandHandler(repositoryMock.Object, mapperMock.Object);
        }

        [TestCase(null, ExpectedResult = typeof(ArgumentNullException))]
        [TestCase("", ExpectedResult = typeof(ArgumentException))]
        [TestCase("user@example.com", ExpectedResult = null)]
        public async Task<Type?> Handle_EmailValidation_ThrowsException(string? email)
        {
            // Arrange
            var command = new GetSlotsByEmailCommand(email, "");

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

        [TestCase("user@example.com", "", 2, Description = "Valid email and slots exist")]
        [TestCase("user@example.com", "", 0, Description = "Valid email and no slots exist")]
        [TestCase("user@example.com", "Test", 1, Description = "Valid email with filter string")]
        public async Task Handle_SlotsHandling_ReturnsExpectedResult(string email, string containsString, int expectedSlotCount)
        {
            // Arrange
            var command = new GetSlotsByEmailCommand(email, containsString);

            var slots = Enumerable.Range(1, expectedSlotCount).Select(i => new ServerSlot
            {
                Id = $"slot-{i}",
                UserEmail = email,
                Name = $"Test Slot {i}"
            }).ToList();

            var expectedResponses = slots.Select(slot => new ServerSlotResponse
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
                    .Setup(m => m.Map<ServerSlotResponse>(slot))
                    .Returns(expectedResponses.First(r => r.Id == slot.Id));
            }

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(expectedSlotCount));

            repositoryMock.Verify(r =>
                r.GetSlotsByUserEmailAsync(email, containsString, It.IsAny<CancellationToken>()), Times.Once);

            foreach (var slot in slots)
            {
                mapperMock.Verify(m => m.Map<ServerSlotResponse>(slot), Times.Once);
            }
        }
    }
}