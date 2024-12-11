using Moq;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.CheckSlotKey.Tests
{
    [TestFixture]
    internal class CheckSlotKeyCommandHandlerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private CheckSlotKeyCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IServerSlotRepository>();
            handler = new CheckSlotKeyCommandHandler(repositoryMock.Object);
        }

        private static IEnumerable<TestCaseData> CheckSlotKeyTestCases()
        {
            yield return new TestCaseData(
                new ServerSlot
                {
                    Id = "1",
                    UserEmail = "user1@example.com",
                    Name = "Slot1",
                    SlotKey = "valid-key"
                },
                "valid-key",
                true
            ).SetDescription("Slot exists with a matching key.");

            yield return new TestCaseData(
                null,
                "invalid-key",
                false
            ).SetDescription("Slot does not exist with the given key.");
        }

        [Test]
        [TestCaseSource(nameof(CheckSlotKeyTestCases))]
        public async Task Handle_TestCases(ServerSlot? slot, string key, bool expectedIsExisting)
        {
            // Arrange
            repositoryMock.Setup(repo => repo.GetSlotByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            var request = new CheckSlotKeyRequest { SlotKey = key };
            var command = new CheckSlotKeyCommand(request);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.IsExisting, Is.EqualTo(expectedIsExisting));
            Assert.That(result.SlotKey, Is.EqualTo(key));

            repositoryMock.Verify(repo => repo.GetSlotByKeyAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}