using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApiTests.Services
{
    [TestFixture]
    internal class SlotDataPickerTests
    {
        private const int MaxEventAmount = 10;

        private Mock<IStatisticsReceiver<ServerStatistics>> mockServerStatisticsReceiver;
        private Mock<IStatisticsReceiver<ServerLoadStatistics>> mockLoadStatisticsReceiver;
        private Mock<IStatisticsReceiver<ServerCustomStatistics>> mockCustomStatisticsReceiver;
        private Mock<IEventReceiver<LoadEventWrapper>> mockLoadEventReceiver;
        private Mock<IEventReceiver<CustomEventWrapper>> mockCustomEventReceiver;
        private Mock<IConfiguration> mockConfiguration;
        private SlotDataPicker slotDataPicker;

        [SetUp]
        public void Setup()
        {
            mockServerStatisticsReceiver = new Mock<IStatisticsReceiver<ServerStatistics>>();
            mockLoadStatisticsReceiver = new Mock<IStatisticsReceiver<ServerLoadStatistics>>();
            mockCustomStatisticsReceiver = new Mock<IStatisticsReceiver<ServerCustomStatistics>>();
            mockLoadEventReceiver = new Mock<IEventReceiver<LoadEventWrapper>>();
            mockCustomEventReceiver = new Mock<IEventReceiver<CustomEventWrapper>>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(c => c[Configuration.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA])
                 .Returns(MaxEventAmount.ToString());
            slotDataPicker = new SlotDataPicker(
                mockServerStatisticsReceiver.Object,
                mockLoadStatisticsReceiver.Object,
                mockCustomStatisticsReceiver.Object,
                mockLoadEventReceiver.Object,
                mockCustomEventReceiver.Object,
                mockConfiguration.Object);
        }
        [Test]
        public async Task GetSlotDataAsync_ValidKey_ReturnsSlotData()
        {
            // Arrange
            var key = "test-key";
            var cancellationToken = CancellationToken.None;
            var expectedServerStatistics = new ServerStatistics();
            var expectedLoadStatistics = new ServerLoadStatistics();
            var expectedCustomStatistics = new ServerCustomStatistics();
            var expectedLoadEvents = new List<LoadEventWrapper>();
            var expectedCustomEvents = new List<CustomEventWrapper>();
            mockServerStatisticsReceiver.Setup(r => r.ReceiveLastStatisticsByKeyAsync(key, cancellationToken))
                                        .ReturnsAsync(expectedServerStatistics);
            mockLoadStatisticsReceiver.Setup(r => r.ReceiveLastStatisticsByKeyAsync(key, cancellationToken))
                                      .ReturnsAsync(expectedLoadStatistics);
            mockCustomStatisticsReceiver.Setup(r => r.ReceiveLastStatisticsByKeyAsync(key, cancellationToken))
                                        .ReturnsAsync(expectedCustomStatistics);
            mockLoadEventReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<ReadCertainMessageNumberOptions>(), cancellationToken))
                                 .ReturnsAsync(expectedLoadEvents);
            mockCustomEventReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<ReadCertainMessageNumberOptions>(), cancellationToken))
                                   .ReturnsAsync(expectedCustomEvents);
            // Act
            var result = await slotDataPicker.GetSlotDataAsync(key, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.GeneralStatistics, Is.EqualTo(expectedServerStatistics));
            Assert.That(result.LoadStatistics, Is.EqualTo(expectedLoadStatistics));
            Assert.That(result.CustomEventStatistics, Is.EqualTo(expectedCustomStatistics));
            Assert.That(result.LastLoadEvents, Is.EqualTo(expectedLoadEvents));
            Assert.That(result.LastCustomEvents, Is.EqualTo(expectedCustomEvents));
        }
        [Test]
        public async Task GetSlotDataAsync_ReceiversReturnNull_ReturnsSlotDataWithNullValues()
        {
            // Arrange
            var key = "test-key";
            var cancellationToken = CancellationToken.None;
            var expectedLoadEvents = new List<LoadEventWrapper>();
            var expectedCustomEvents = new List<CustomEventWrapper>();

            mockServerStatisticsReceiver.Setup(r => r.ReceiveLastStatisticsByKeyAsync(key, cancellationToken))
                                        .ReturnsAsync((ServerStatistics)null);
            mockLoadStatisticsReceiver.Setup(r => r.ReceiveLastStatisticsByKeyAsync(key, cancellationToken))
                                      .ReturnsAsync((ServerLoadStatistics)null);
            mockCustomStatisticsReceiver.Setup(r => r.ReceiveLastStatisticsByKeyAsync(key, cancellationToken))
                                        .ReturnsAsync((ServerCustomStatistics)null);
            mockLoadEventReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<ReadCertainMessageNumberOptions>(), cancellationToken))
                                 .ReturnsAsync(expectedLoadEvents);
            mockCustomEventReceiver.Setup(r => r.GetCertainAmountOfEventsAsync(It.IsAny<ReadCertainMessageNumberOptions>(), cancellationToken))
                                   .ReturnsAsync(expectedCustomEvents);
            // Act
            var result = await slotDataPicker.GetSlotDataAsync(key, cancellationToken);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.GeneralStatistics);
            Assert.IsNull(result.LoadStatistics);
            Assert.IsNull(result.CustomEventStatistics);
            Assert.That(result.LastLoadEvents, Is.EqualTo(expectedLoadEvents));
            Assert.That(result.LastCustomEvents, Is.EqualTo(expectedCustomEvents));
        }
    }
}