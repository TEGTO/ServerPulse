using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerMonitorApi.Controllers;
using ServerMonitorApi.Services;

namespace ServerMonitorApiTests.Controllers
{
    [TestFixture]
    internal class StatisticsControlControllerTests
    {
        private Mock<IEventSender> mockMessageSender;
        private Mock<ISlotKeyChecker> mockServerSlotChecker;
        private Mock<IStatisticsControlService> mockStatisticsControlService;
        private StatisticsControlController controller;

        [SetUp]
        public void Setup()
        {
            mockMessageSender = new Mock<IEventSender>();
            mockServerSlotChecker = new Mock<ISlotKeyChecker>();
            mockStatisticsControlService = new Mock<IStatisticsControlService>();
            controller = new StatisticsControlController(
                mockMessageSender.Object,
                mockServerSlotChecker.Object,
                mockStatisticsControlService.Object
            );
        }

        [Test]
        public async Task DeleteStatisticsByKey_ShouldCallDeleteStatisticsByKeyAsync()
        {
            // Arrange
            var key = "some-key";
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.DeleteStatisticsByKey(key, cancellationToken);
            // Assert
            mockStatisticsControlService.Verify(
                x => x.DeleteStatisticsByKeyAsync(key),
                Times.Once
            );
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task DeleteStatisticsByKey_ShouldReturnOkResult()
        {
            // Arrange
            var key = "some-key";
            var cancellationToken = CancellationToken.None;
            // Act
            var result = await controller.DeleteStatisticsByKey(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
    }
}
