using Helper.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerSlotApi.Dtos;
using System.Text.Json;

namespace ServerMonitorApi.Services.Tests
{
    [TestFixture]
    internal class SlotKeyCheckerTests
    {
        private Mock<IHttpHelper> httpHelperMock;
        private Mock<IConfiguration> configurationMock;
        private SlotKeyChecker slotKeyChecker;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            httpHelperMock = new Mock<IHttpHelper>();
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c[ConfigurationKeys.SERVER_SLOT_URL]).Returns("http://api.gateway/");
            configurationMock.Setup(c => c[ConfigurationKeys.SERVER_SLOT_ALIVE_CHECKER]).Returns("server/slot/check");

            slotKeyChecker = new SlotKeyChecker(httpHelperMock.Object, configurationMock.Object);
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
            var expectedUrl = "http://api.gateway/server/slot/check";
            var expectedRequestBody = JsonSerializer.Serialize(new CheckSlotKeyRequest { SlotKey = key });

            httpHelperMock
                .Setup(h => h.SendPostRequestAsync<CheckSlotKeyResponse>(
                    expectedUrl,
                    expectedRequestBody,
                    null,
                    cancellationToken))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await slotKeyChecker.CheckSlotKeyAsync(key, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
            httpHelperMock.Verify(h => h.SendPostRequestAsync<CheckSlotKeyResponse>(
                expectedUrl,
                expectedRequestBody,
                null,
                cancellationToken), Times.Once);
        }

        [Test]
        public async Task CheckSlotKeyAsync_NullResponse_ReturnsFalse()
        {
            // Arrange
            var key = "keyWithNoResponse";
            var expectedUrl = "http://api.gateway/server/slot/check";
            var expectedRequestBody = JsonSerializer.Serialize(new CheckSlotKeyRequest { SlotKey = key });

            httpHelperMock
                .Setup(h => h.SendPostRequestAsync<CheckSlotKeyResponse>(
                    expectedUrl,
                    expectedRequestBody,
                    null,
                    cancellationToken))
                .ReturnsAsync((CheckSlotKeyResponse?)null);

            // Act
            var result = await slotKeyChecker.CheckSlotKeyAsync(key, cancellationToken);

            // Assert
            Assert.That(result, Is.False);
            httpHelperMock.Verify(h => h.SendPostRequestAsync<CheckSlotKeyResponse>(
                expectedUrl,
                expectedRequestBody,
                null,
                cancellationToken), Times.Once);
        }
    }
}