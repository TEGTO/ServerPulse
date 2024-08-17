using CacheUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CacheUtilsTests
{
    [TestFixture]
    public class CacheExtensionsTests
    {
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IServiceCollection> mockServices;

        [SetUp]
        public void Setup()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockServices = new Mock<IServiceCollection>();
        }

        //[Test]
        //public void AddCache_ShouldRegisterRedisAndCacheService()
        //{
        //    // Arrange
        //    var redisConnectionString = "localhost:6379";
        //    mockConfiguration.Setup(c => c.GetSection("ConnectionStrings")[It.IsAny<string>()]).Returns(redisConnectionString);
        //    var services = new ServiceCollection();
        //    // Act
        //    services.AddCache(mockConfiguration.Object);
        //    // Assert
        //    var serviceProvider = services.BuildServiceProvider();
        //    var connectionMultiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
        //    Assert.IsNotNull(connectionMultiplexer);
        //    Assert.IsInstanceOf<ConnectionMultiplexer>(connectionMultiplexer);
        //    var cacheService = serviceProvider.GetService<ICacheService>();
        //    Assert.IsNotNull(cacheService);
        //    Assert.IsInstanceOf<RedisService>(cacheService);
        //}
        [Test]
        public void AddCache_WithInvalidConnectionString_ThrowsException()
        {
            // Arrange
            mockConfiguration.Setup(x => x.GetSection(It.Is<string>(s => s == Configuration.REDIS_CONNECTION_STRING)))
                             .Returns((IConfigurationSection?)null);
            var services = new ServiceCollection();
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => services.AddCache(mockConfiguration.Object));
        }
    }
}
