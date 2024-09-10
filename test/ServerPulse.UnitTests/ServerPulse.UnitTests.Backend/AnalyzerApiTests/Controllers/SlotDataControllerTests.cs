using AnalyzerApi;
using AnalyzerApi.Controllers;
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace AnalyzerApi.Controllers.Tests
{
    [TestFixture]
    public class SlotDataControllerTests
    {
        private Mock<ISlotDataPicker> mockDataPicker;
        private Mock<IMapper> mockMapper;
        private Mock<ICacheService> mockCacheService;
        private Mock<IConfiguration> mockConfiguration;
        private SlotDataController slotDataController;

        private const double CacheExpiryInMinutes = 10.0;
        private const string CacheKey = "test-cache-key";

        [SetUp]
        public void Setup()
        {
            mockDataPicker = new Mock<ISlotDataPicker>();
            mockMapper = new Mock<IMapper>();
            mockCacheService = new Mock<ICacheService>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.Setup(c => c[Configuration.CACHE_EXPIRY_IN_MINUTES])
                             .Returns(CacheExpiryInMinutes.ToString());
            mockConfiguration.Setup(c => c[Configuration.CACHE_KEY])
                             .Returns(CacheKey);

            slotDataController = new SlotDataController(
                mockDataPicker.Object,
                mockMapper.Object,
                mockCacheService.Object,
                mockConfiguration.Object);
        }

        [Test]
        public async Task GetData_KeyExistsInCache_ReturnsCachedData()
        {
            // Arrange
            var key = "test-key";
            var cacheKey = $"{CacheKey}-{key}-slotdata";
            var cancellationToken = CancellationToken.None;
            var cachedSlotData = new SlotData() { LastCustomEvents = null, LastLoadEvents = null };
            var expectedResponse = new SlotDataResponse();
            var cachedJson = JsonSerializer.Serialize(cachedSlotData);
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync(cachedJson);
            mockMapper.Setup(m => m.Map<SlotDataResponse>(It.IsAny<SlotData>()))
                      .Returns(expectedResponse);
            // Act
            var result = await slotDataController.GetData(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.AreEqual(expectedResponse, okResult.Value);
        }
        [Test]
        public async Task GetData_KeyNotInCache_FetchesDataAndStoresInCache()
        {
            // Arrange
            var key = "test-key";
            var cacheKey = $"{CacheKey}-{key}-slotdata";
            var cancellationToken = CancellationToken.None;
            var slotData = new SlotData() { LastCustomEvents = null, LastLoadEvents = null };
            var expectedResponse = new SlotDataResponse();
            mockCacheService.Setup(x => x.GetValueAsync(cacheKey)).ReturnsAsync((string)null);
            mockDataPicker.Setup(dp => dp.GetSlotDataAsync(key, cancellationToken))
                          .ReturnsAsync(slotData);
            mockCacheService.Setup(c => c.SetValueAsync(CacheKey, It.IsAny<string>(), CacheExpiryInMinutes))
                            .Returns(Task.CompletedTask);
            mockMapper.Setup(m => m.Map<SlotDataResponse>(slotData))
                      .Returns(expectedResponse);
            // Act
            var result = await slotDataController.GetData(key, cancellationToken);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
            mockCacheService.Verify(c => c.SetValueAsync(cacheKey, It.IsAny<string>(), CacheExpiryInMinutes), Times.Once);
        }
    }
}
