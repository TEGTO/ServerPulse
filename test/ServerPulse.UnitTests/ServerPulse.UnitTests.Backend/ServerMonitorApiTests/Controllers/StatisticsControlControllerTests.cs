using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServerMonitorApi.Command.DeleteStatisticsByKey;

namespace ServerMonitorApi.Controllers.Tests
{
    [TestFixture]
    internal class StatisticsControlControllerTests
    {
        private Mock<IMediator> mediatorMock;
        private StatisticsControlController controller;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            mediatorMock = new Mock<IMediator>();

            controller = new StatisticsControlController(mediatorMock.Object);
            cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task DeleteStatisticsByKey_ValidKey_ReturnsOk()
        {
            // Arrange
            var key = "validKey";
            mediatorMock.Setup(m => m.Send(It.IsAny<DeleteStatisticsByKeyCommand>(), cancellationToken))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await controller.DeleteStatisticsByKey(key, cancellationToken);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());
            mediatorMock.Verify(m => m.Send(It.Is<DeleteStatisticsByKeyCommand>(c => c.Key == key), cancellationToken), Times.Once);
        }
    }
}