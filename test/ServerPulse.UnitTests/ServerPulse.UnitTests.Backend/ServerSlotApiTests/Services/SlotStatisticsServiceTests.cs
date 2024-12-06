using Microsoft.Extensions.Configuration;
using Moq;
using Shared.Helpers;

namespace ServerSlotApi.Services.Tests
{
    [TestFixture]
    internal class SlotStatisticsServiceTests
    {
        private Mock<IHttpHelper> httpHelperMock;
        private Mock<IConfiguration> configurationMock;
        private SlotStatisticsService service;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            httpHelperMock = new Mock<IHttpHelper>();
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c[Configuration.API_GATEWAY]).Returns("http://api.gateway/");
            configurationMock.Setup(c => c[Configuration.STATISTICS_DELETE_URL]).Returns("statistics/delete/");

            service = new SlotStatisticsService(httpHelperMock.Object, configurationMock.Object);
            cancellationToken = CancellationToken.None;
        }

        private static IEnumerable<TestCaseData> ValidDeleteRequestTestCases()
        {
            yield return new TestCaseData("testKey1", "validToken1", "http://api.gateway/statistics/delete/testKey1")
                .SetDescription("Valid key and token should result in a valid delete request.");
            yield return new TestCaseData("anotherKey", "tokenValue", "http://api.gateway/statistics/delete/anotherKey")
                .SetDescription("Another valid key and token should result in a valid delete request.");
        }

        [Test]
        [TestCaseSource(nameof(ValidDeleteRequestTestCases))]
        public async Task DeleteSlotStatisticsAsync_ValidKeyAndToken_SendsDeleteRequest(string key, string token, string expectedUrl)
        {
            // Arrange
            httpHelperMock
                .Setup(h => h.SendDeleteRequestAsync(expectedUrl, token, cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await service.DeleteSlotStatisticsAsync(key, token, cancellationToken);

            // Assert
            httpHelperMock.Verify(h => h.SendDeleteRequestAsync(expectedUrl, token, cancellationToken), Times.Once);
        }
    }
}