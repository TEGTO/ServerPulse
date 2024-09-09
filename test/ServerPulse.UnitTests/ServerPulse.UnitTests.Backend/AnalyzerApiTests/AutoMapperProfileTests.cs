using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AuthenticationApi;
using AutoMapper;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApiTests
{
    [TestFixture]
    internal class AutoMapperProfileTests
    {
        private IMapper mapper;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            mapper = config.CreateMapper();
        }

        [Test]
        public void AutoMapperProfile_ConfigurationEventMapping_MapsToConfigurationEventWrapper()
        {
            // Arrange
            var configEvent = new ConfigurationEvent("TestKey", TimeSpan.FromMinutes(5));
            // Act
            var result = mapper.Map<ConfigurationEventWrapper>(configEvent);
            // Assert
            Assert.That(result.Key, Is.EqualTo(configEvent.Key));
            Assert.That(result.ServerKeepAliveInterval, Is.EqualTo(configEvent.ServerKeepAliveInterval));
        }
        [Test]
        public void AutoMapperProfile_PulseEventMapping_MapsToPulseEventWrapper()
        {
            // Arrange
            var pulseEvent = new PulseEvent("TestKey", true);
            // Act
            var result = mapper.Map<PulseEventWrapper>(pulseEvent);
            // Assert
            Assert.That(result.Key, Is.EqualTo(pulseEvent.Key));
            Assert.That(result.IsAlive, Is.EqualTo(pulseEvent.IsAlive));
        }
        [Test]
        public void AutoMapperProfile_LoadEventMapping_MapsToLoadEventWrapper()
        {
            // Arrange
            var loadEvent = new LoadEvent("TestKey", "Endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.UtcNow);
            // Act
            var result = mapper.Map<LoadEventWrapper>(loadEvent);
            // Assert
            Assert.That(result.Key, Is.EqualTo(loadEvent.Key));
            Assert.That(result.Endpoint, Is.EqualTo(loadEvent.Endpoint));
            Assert.That(result.Method, Is.EqualTo(loadEvent.Method));
            Assert.That(result.StatusCode, Is.EqualTo(loadEvent.StatusCode));
            Assert.That(result.Duration, Is.EqualTo(loadEvent.Duration));
            Assert.That(result.TimestampUTC, Is.EqualTo(loadEvent.TimestampUTC));
        }
        [Test]
        public void AutoMapperProfile_LoadMethodStatisticsMapping_MapsToLoadMethodStatisticsResponse()
        {
            // Arrange
            var methodStats = new LoadMethodStatistics
            {
                GetAmount = 5,
                PostAmount = 3,
                PutAmount = 2,
                PatchAmount = 1,
                DeleteAmount = 0
            };
            // Act
            var result = mapper.Map<LoadMethodStatisticsResponse>(methodStats);
            // Assert
            Assert.That(result.GetAmount, Is.EqualTo(methodStats.GetAmount));
            Assert.That(result.PostAmount, Is.EqualTo(methodStats.PostAmount));
            Assert.That(result.PutAmount, Is.EqualTo(methodStats.PutAmount));
            Assert.That(result.PatchAmount, Is.EqualTo(methodStats.PatchAmount));
            Assert.That(result.DeleteAmount, Is.EqualTo(methodStats.DeleteAmount));
        }
        [Test]
        public void AutoMapperProfile_ServerStatisticsMapping_MapsToServerStatisticsResponse()
        {
            // Arrange
            var serverStats = new ServerStatistics
            {
                IsAlive = true,
                DataExists = true,
                ServerLastStartDateTimeUTC = DateTime.UtcNow.AddHours(-2),
                ServerUptime = TimeSpan.FromHours(2),
                LastServerUptime = TimeSpan.FromHours(1),
                LastPulseDateTimeUTC = DateTime.UtcNow.AddMinutes(-10)
            };
            // Act
            var result = mapper.Map<ServerStatisticsResponse>(serverStats);
            // Assert
            Assert.That(result.IsAlive, Is.EqualTo(serverStats.IsAlive));
            Assert.That(result.DataExists, Is.EqualTo(serverStats.DataExists));
            Assert.That(result.ServerLastStartDateTimeUTC, Is.EqualTo(serverStats.ServerLastStartDateTimeUTC));
            Assert.That(result.ServerUptime, Is.EqualTo(serverStats.ServerUptime));
            Assert.That(result.LastServerUptime, Is.EqualTo(serverStats.LastServerUptime));
            Assert.That(result.LastPulseDateTimeUTC, Is.EqualTo(serverStats.LastPulseDateTimeUTC));
        }
        [Test]
        public void AutoMapperProfile_LoadAmountStatisticsMapping_MapsToLoadAmountStatisticsResponse()
        {
            // Arrange
            var loadAmountStats = new LoadAmountStatistics
            {
                AmountOfEvents = 100,
                DateFrom = DateTime.UtcNow.AddDays(-1),
                DateTo = DateTime.UtcNow
            };
            // Act
            var result = mapper.Map<LoadAmountStatisticsResponse>(loadAmountStats);
            // Assert
            Assert.That(result.AmountOfEvents, Is.EqualTo(loadAmountStats.AmountOfEvents));
            Assert.That(result.DateFrom, Is.EqualTo(loadAmountStats.DateFrom));
            Assert.That(result.DateTo, Is.EqualTo(loadAmountStats.DateTo));
        }
        [Test]
        public void AutoMapperProfile_CustomEventMapping_MapsToCustomEventWrapper()
        {
            // Arrange
            var customEvent = new CustomEvent("TestKey", "TestName", "TestDescription");
            // Act
            var result = mapper.Map<CustomEventWrapper>(customEvent);
            // Assert
            Assert.That(result.Key, Is.EqualTo(customEvent.Key));
            Assert.That(result.Name, Is.EqualTo(customEvent.Name));
            Assert.That(result.Description, Is.EqualTo(customEvent.Description));
        }
        [Test]
        public void AutoMapperProfile_CustomEventStatisticsMapping_MapsToCustomEventStatisticsResponse()
        {
            // Arrange
            var customEventStats = new ServerCustomStatistics
            {
                LastEvent = new CustomEventWrapper
                {
                    Id = "123",
                    Key = "TestKey",
                    Name = "TestName",
                    Description = "TestDescription"
                }
            };
            // Act
            var result = mapper.Map<CustomEventStatisticsResponse>(customEventStats);
            // Assert
            Assert.NotNull(result.LastEvent);
            Assert.That(result.LastEvent.Key, Is.EqualTo(customEventStats.LastEvent.Key));
            Assert.That(result.LastEvent.Name, Is.EqualTo(customEventStats.LastEvent.Name));
            Assert.That(result.LastEvent.Description, Is.EqualTo(customEventStats.LastEvent.Description));
        }
        [Test]
        public void AutoMapperProfile_SlotDataMapping_MapsToSlotDataResponse()
        {
            // Arrange
            var slotData = new SlotData
            {
                GeneralStatistics = new ServerStatistics { IsAlive = true },
                LoadStatistics = new ServerLoadStatistics { AmountOfEvents = 5 },
                CustomEventStatistics = new ServerCustomStatistics(),
                LastLoadEvents = new List<LoadEventWrapper> { new LoadEventWrapper() },
                LastCustomEvents = new List<CustomEventWrapper> { new CustomEventWrapper() }
            };
            // Act
            var result = mapper.Map<SlotDataResponse>(slotData);
            // Assert
            Assert.NotNull(result.GeneralStatistics);
            Assert.NotNull(result.LoadStatistics);
            Assert.NotNull(result.CustomEventStatistics);
            Assert.IsNotEmpty(result.LastLoadEvents);
            Assert.IsNotEmpty(result.LastCustomEvents);
        }
    }
}