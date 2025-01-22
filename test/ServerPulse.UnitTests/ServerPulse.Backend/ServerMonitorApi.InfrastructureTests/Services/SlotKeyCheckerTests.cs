using Moq;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey;

namespace ServerMonitorApi.Infrastructure.Services.Tests
{
    [TestFixture]
    internal class SlotKeyCheckerTests
    {
        private Mock<IServerSlotApi> serverSlotApiMock;
        private SlotKeyChecker slotKeyChecker;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            serverSlotApiMock = new Mock<IServerSlotApi>();

            slotKeyChecker = new SlotKeyChecker(serverSlotApiMock.Object);
            cancellationToken = CancellationToken.None;
        }

        private static IEnumerable<TestCaseData> CheckSlotKeyTestCases()
        {
            yield return new TestCaseData(
                "validKey",
                new CheckSlotKeyResponse { SlotKey = "validKey", IsExisting = true },
                true
            ).SetDescription("Valid slot key should return true.");

            yield return new TestCaseData(
                "nonExistentKey",
                new CheckSlotKeyResponse { SlotKey = "nonExistentKey", IsExisting = false },
                false
            ).SetDescription("Non-existent slot key should return false.");
        }

        [Test]
        [TestCaseSource(nameof(CheckSlotKeyTestCases))]
        public async Task CheckSlotKeyAsync_ValidResponses_ReturnsExpectedResult(string key, CheckSlotKeyResponse httpResponse, bool expectedResult)
        {
            // Arrange
            serverSlotApiMock
                .Setup(h => h.CheckSlotKeyAsync(It.IsAny<CheckSlotKeyRequest>(), cancellationToken))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await slotKeyChecker.CheckSlotKeyAsync(key, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
            serverSlotApiMock.Verify(h => h.CheckSlotKeyAsync(It.IsAny<CheckSlotKeyRequest>(), cancellationToken), Times.Once);
        }

        [Test]
        public async Task CheckSlotKeyAsync_NullResponse_ReturnsFalse()
        {
            // Arrange
            var key = "keyWithNoResponse";

            // Act
            var result = await slotKeyChecker.CheckSlotKeyAsync(key, cancellationToken);

            // Assert
            Assert.That(result, Is.False);
            serverSlotApiMock.Verify(h => h.CheckSlotKeyAsync(It.IsAny<CheckSlotKeyRequest>(), cancellationToken), Times.Once);
        }
    }
}