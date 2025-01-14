using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using System.Globalization;

namespace AnalyzerApi.Application.Tests
{
    [TestFixture]
    internal class AutoMapperProfileTests
    {
        private IMapper mapper;
        private MapperConfiguration configuration;

        [SetUp]
        public void SetUp()
        {
            configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            mapper = configuration.CreateMapper();
        }

        [TestCase(typeof(ConfigurationEvent), typeof(ConfigurationEventWrapper), "testKey", "00:10:00")]
        [TestCase(typeof(PulseEvent), typeof(PulseEventWrapper), "testKey", true)]
        [TestCase(typeof(LoadEvent), typeof(LoadEventWrapper), "testKey", "/endpoint", "GET", 200, "00:00:10", "2024-01-01T00:00:00Z")]
        [TestCase(typeof(CustomEvent), typeof(CustomEventWrapper), "testKey", "Custom Name", "Custom Description")]
        public void Map_BaseEvent_To_Wrapper(Type sourceType, Type destinationType, params object[] constructorArgs)
        {
            // Arrange
            for (int i = 0; i < constructorArgs.Length; i++)
            {
                if (constructorArgs[i] is string str)
                {
                    if (TimeSpan.TryParse(str, new CultureInfo("en-US"), out var timeSpan))
                    {
                        constructorArgs[i] = timeSpan;
                    }
                    else if (DateTime.TryParse(str, new CultureInfo("en-US"), out var dateTime))
                    {
                        constructorArgs[i] = dateTime;
                    }
                }
            }

            var source = Activator.CreateInstance(sourceType, constructorArgs);

            // Act
            var result = mapper.Map(source, sourceType, destinationType);

            // Assert
            Assert.IsInstanceOf(destinationType, result);
        }

        [TestCase(typeof(ConfigurationEventWrapper), typeof(ConfigurationEventResponse))]
        [TestCase(typeof(PulseEventWrapper), typeof(PulseEventResponse))]
        [TestCase(typeof(LoadEventWrapper), typeof(LoadEventResponse))]
        [TestCase(typeof(CustomEventWrapper), typeof(CustomEventResponse))]
        public void Map_EventWrapper_To_Response(Type sourceType, Type destinationType)
        {
            // Arrange
            var source = Activator.CreateInstance(sourceType);
            if (source is BaseEventWrapper baseWrapper)
            {
                baseWrapper.Id = Guid.NewGuid().ToString();
                baseWrapper.Key = "testKey";
                baseWrapper.CreationDateUTC = DateTime.UtcNow;
            }

            // Act
            var result = mapper.Map(source, sourceType, destinationType);

            // Assert
            Assert.IsInstanceOf(destinationType, result);
        }

        [TestCase(typeof(LoadMethodStatistics), typeof(LoadMethodStatisticsResponse))]
        [TestCase(typeof(ServerLifecycleStatistics), typeof(ServerLifecycleStatisticsResponse))]
        [TestCase(typeof(ServerLoadStatistics), typeof(ServerLoadStatisticsResponse))]
        [TestCase(typeof(LoadAmountStatistics), typeof(LoadAmountStatisticsResponse))]
        [TestCase(typeof(ServerCustomStatistics), typeof(ServerCustomStatisticsResponse))]
        public void Map_Statistics_To_StatisticsResponse(Type sourceType, Type destinationType)
        {
            // Arrange
            var source = Activator.CreateInstance(sourceType);

            // Act
            var result = mapper.Map(source, sourceType, destinationType);

            // Assert
            Assert.IsInstanceOf(destinationType, result);
        }

        [Test]
        public void Map_ConfigurationEvent_To_ConfigurationEventWrapper_ValidData()
        {
            // Arrange
            var source = new ConfigurationEvent("testKey", TimeSpan.FromMinutes(30));

            // Act
            var result = mapper.Map<ConfigurationEventWrapper>(source);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Key, Is.EqualTo(source.Key));
            Assert.That(result.ServerKeepAliveInterval, Is.EqualTo(source.ServerKeepAliveInterval));
        }

        [Test]
        public void Map_PulseEvent_To_PulseEventWrapper_ValidData()
        {
            // Arrange
            var source = new PulseEvent("testKey", true);

            // Act
            var result = mapper.Map<PulseEventWrapper>(source);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Key, Is.EqualTo(source.Key));
            Assert.That(result.IsAlive, Is.EqualTo(source.IsAlive));
        }

        [Test]
        public void Map_LoadEvent_To_LoadEventWrapper_ValidData()
        {
            // Arrange
            var source = new LoadEvent("testKey", "/api/test", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow);

            // Act
            var result = mapper.Map<LoadEventWrapper>(source);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Key, Is.EqualTo(source.Key));
            Assert.That(result.Endpoint, Is.EqualTo(source.Endpoint));
            Assert.That(result.Method, Is.EqualTo(source.Method));
            Assert.That(result.StatusCode, Is.EqualTo(source.StatusCode));
            Assert.That(result.Duration, Is.EqualTo(source.Duration));
            Assert.That(result.TimestampUTC, Is.EqualTo(source.TimestampUTC));
        }

        [Test]
        public void Map_CustomEvent_To_CustomEventWrapper_ValidData()
        {
            // Arrange
            var source = new CustomEvent("testKey", "CustomName", "CustomDescription");

            // Act
            var result = mapper.Map<CustomEventWrapper>(source);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Key, Is.EqualTo(source.Key));
            Assert.That(result.Name, Is.EqualTo(source.Name));
            Assert.That(result.Description, Is.EqualTo(source.Description));
        }
    }
}