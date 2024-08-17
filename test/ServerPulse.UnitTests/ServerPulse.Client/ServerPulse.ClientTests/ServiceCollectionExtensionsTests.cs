using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Services;

namespace ServerPulse.ClientTests
{
    [TestFixture]
    public class ServiceCollectionExtensionsTests
    {
        private IServiceCollection services;
        private Mock<ServerPulseSettings> settingsMock;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();
            settingsMock = new Mock<ServerPulseSettings>();
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
        public void AddServerPulseClient_RegistersServerLoadSender()
        {
            // Act
            services.AddServerPulseClient(settingsMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var serverLoadSender = serviceProvider.GetService<ServerLoadSender>();
            Assert.IsNotNull(serverLoadSender);
            var iServerLoadSender = serviceProvider.GetService<IServerLoadSender>();
            Assert.IsNotNull(iServerLoadSender);
            Assert.That(iServerLoadSender, Is.SameAs(serverLoadSender));
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
            Assert.IsTrue(hostedServices.Any(hs => hs is ServerLoadSender));
            Assert.IsTrue(hostedServices.Any(hs => hs is ServerStatusSender));
        }
    }
}