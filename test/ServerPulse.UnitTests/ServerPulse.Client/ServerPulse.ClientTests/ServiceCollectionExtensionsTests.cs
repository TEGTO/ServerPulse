using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using ServerPulse.Client.Services;
using ServerPulse.Client.Services.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Tests
{
    [TestFixture]
    public class ServiceCollectionExtensionsTests
    {
        private IServiceCollection services;
        private Mock<EventSendingSettings> settingsMock;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();
            settingsMock = new Mock<EventSendingSettings>();
        }

        [Test]
        public void AddServerPulseClient_RegistersHttpClient()
        {
            // Act
            services.AddServerPulseClient(settingsMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNotNull(httpClientFactory);
        }
        [Test]
        public void AddServerPulseClient_RegistersMessageSender()
        {
            // Act
            services.AddServerPulseClient(settingsMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var messageSender = serviceProvider.GetService<IMessageSender>();
            Assert.IsNotNull(messageSender);
            Assert.IsInstanceOf<MessageSender>(messageSender);
        }
        [Test]
        public void AddServerPulseClient_RegistersEventSendingSettings()
        {
            // Act
            services.AddServerPulseClient(settingsMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var pulseSettings = serviceProvider.GetService<EventSendingSettings<PulseEvent>>();
            var configSettings = serviceProvider.GetService<EventSendingSettings<ConfigurationEvent>>();
            var loadSettings = serviceProvider.GetService<EventSendingSettings<LoadEvent>>();
            var customSettings = serviceProvider.GetService<EventSendingSettings<CustomEvent>>();
            Assert.IsNotNull(pulseSettings);
            Assert.IsNotNull(configSettings);
            Assert.IsNotNull(loadSettings);
            Assert.IsNotNull(customSettings);
            // Validate that the settings are correctly configured
            Assert.That(pulseSettings.EventController, Is.EqualTo("/serverinteraction/pulse"));
            Assert.That(configSettings.EventController, Is.EqualTo("/serverinteraction/configuration"));
            Assert.That(loadSettings.EventController, Is.EqualTo("/serverinteraction/load"));
            Assert.That(customSettings.EventController, Is.EqualTo("/serverinteraction/custom"));
        }
        [Test]
        public void AddServerPulseClient_RegistersQueueMessageSenders()
        {
            // Act
            services.AddServerPulseClient(settingsMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var loadQueueSender = serviceProvider.GetService<QueueMessageSender<LoadEvent>>();
            var loadSenderInterface = serviceProvider.GetService<IQueueMessageSender<LoadEvent>>();
            var customQueueSender = serviceProvider.GetService<CustomEventSender>();
            var customSenderInterface = serviceProvider.GetService<IQueueMessageSender<CustomEvent>>();
            Assert.IsNotNull(loadQueueSender);
            Assert.IsNotNull(loadSenderInterface);
            Assert.That(loadSenderInterface, Is.SameAs(loadQueueSender));
            Assert.IsNotNull(customQueueSender);
            Assert.IsNotNull(customSenderInterface);
            Assert.That(customSenderInterface, Is.SameAs(customQueueSender));
        }
        [Test]
        public void AddServerPulseClient_RegistersHostedServices()
        {
            // Act
            services.AddServerPulseClient(settingsMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var hostedServices = serviceProvider.GetServices<IHostedService>();
            Assert.IsNotNull(hostedServices);
            Assert.IsTrue(hostedServices.Any(hs => hs is QueueMessageSender<LoadEvent>));
            Assert.IsTrue(hostedServices.Any(hs => hs is CustomEventSender));
            Assert.IsTrue(hostedServices.Any(hs => hs is ServerStatusSender));
        }
    }
}