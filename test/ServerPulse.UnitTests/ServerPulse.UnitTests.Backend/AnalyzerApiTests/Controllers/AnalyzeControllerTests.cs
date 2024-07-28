using AnalyzerApi.Controllers;
using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AnalyzerApiTests.Controllers
{
    [TestFixture]
    internal class AnalyzeControllerTests
    {
        private Mock<IMapper> mockMapper;
        private Mock<IServerAnalyzer> mockServerAnalyzer;
        private AnalyzeController controller;

        [SetUp]
        public void SetUp()
        {
            mockMapper = new Mock<IMapper>();
            mockServerAnalyzer = new Mock<IServerAnalyzer>();
            controller = new AnalyzeController(mockMapper.Object, mockServerAnalyzer.Object);
        }

        [Test]
        public async Task GetCurrentServerStatusByKey_InvalidKey_ReturnsBadRequest()
        {
            // Arrange
            var invalidKey = string.Empty;
            var cancellationToken = CancellationToken.None;
            // Act
            var result = (await controller.GetCurrentServerStatisticsByKey(invalidKey, cancellationToken)).Result;
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid key!"));
        }
        [Test]
        public async Task GetCurrentServerStatusByKey_ValidKey_ReturnsOkWithMappedResponse()
        {
            // Arrange
            var validKey = "valid-key";
            var serverStatus = new ServerStatistics { IsAlive = true };
            var analyzedDataResponse = new ServerStatisticsResponse { IsAlive = true };
            var cancellationToken = CancellationToken.None;
            mockServerAnalyzer
                .Setup(sa => sa.GetServerStatisticsByKeyAsync(validKey, cancellationToken))
                .ReturnsAsync(serverStatus);
            mockMapper
                .Setup(m => m.Map<ServerStatisticsResponse>(serverStatus))
                .Returns(analyzedDataResponse);
            // Act
            var result = (await controller.GetCurrentServerStatisticsByKey(validKey, cancellationToken)).Result;
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(analyzedDataResponse));
        }
        [Test]
        public async Task GetCurrentServerStatusByKey_ValidKey_ServerAnalyzerCalled()
        {
            // Arrange
            var validKey = "valid-key";
            var serverStatus = new ServerStatistics { IsAlive = true };
            var cancellationToken = CancellationToken.None;
            mockServerAnalyzer
                .Setup(sa => sa.GetServerStatisticsByKeyAsync(validKey, cancellationToken))
                .ReturnsAsync(serverStatus);
            // Act
            await controller.GetCurrentServerStatisticsByKey(validKey, cancellationToken);
            // Assert
            mockServerAnalyzer.Verify(sa => sa.GetServerStatisticsByKeyAsync(validKey, cancellationToken), Times.Once);
        }
        [Test]
        public async Task GetCurrentServerStatusByKey_ValidKey_MapperCalled()
        {
            // Arrange
            var validKey = "valid-key";
            var serverStatus = new ServerStatistics { IsAlive = true };
            var analyzedDataResponse = new ServerStatisticsResponse { IsAlive = true };
            var cancellationToken = CancellationToken.None;
            mockServerAnalyzer
                .Setup(sa => sa.GetServerStatisticsByKeyAsync(validKey, cancellationToken))
                .ReturnsAsync(serverStatus);
            mockMapper
                .Setup(m => m.Map<ServerStatisticsResponse>(serverStatus))
                .Returns(analyzedDataResponse);
            // Act
            await controller.GetCurrentServerStatisticsByKey(validKey, cancellationToken);
            // Assert
            mockMapper.Verify(m => m.Map<ServerStatisticsResponse>(serverStatus), Times.Once);
        }
    }
}