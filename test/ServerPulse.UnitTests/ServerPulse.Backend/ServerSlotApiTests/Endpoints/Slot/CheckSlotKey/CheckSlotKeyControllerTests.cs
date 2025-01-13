using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey;
using ServerSlotApi.Core.Entities;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Endpoints.Slot.CheckSlotKey.Tests
{
    [TestFixture]
    internal class CheckSlotKeyControllerTests
    {
        private Mock<IServerSlotRepository> repositoryMock;
        private CheckSlotKeyController controller;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IServerSlotRepository>();

            controller = new CheckSlotKeyController(repositoryMock.Object);
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
            ).SetDescription("Slot exists with a matching key returns Ok.");

            yield return new TestCaseData(
                null,
                "invalid-key",
                false
            ).SetDescription("Slot does not exist with the given key returns Ok, but with not found field.");
        }

        [Test]
        [TestCaseSource(nameof(CheckSlotKeyTestCases))]
        public async Task CheckSlotKey_TestCases(ServerSlot? slot, string key, bool expectedIsExisting)
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = key };

            repositoryMock.Setup(repo => repo.GetSlotByKeyAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slot);

            // Act
            var result = await controller.CheckSlotKey(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as CheckSlotKeyResponse;
            Assert.IsNotNull(response);

            Assert.That(response.IsExisting, Is.EqualTo(expectedIsExisting));
            Assert.That(response.SlotKey, Is.EqualTo(key));

            repositoryMock.Verify(repo => repo.GetSlotByKeyAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}