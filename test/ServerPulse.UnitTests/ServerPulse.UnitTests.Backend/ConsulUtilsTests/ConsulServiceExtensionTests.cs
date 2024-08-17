using Consul;
using ConsulUtils.Configuration;
using ConsulUtils.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;


namespace ConsulUtilsTests
{
    [TestFixture]
    internal class ConsulConfigurationExtensionTests
    {
        private Mock<IConfigurationBuilder> configurationBuilderMock;
        private ConsulSettings configuration;
        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void SetUp()
        {
            configurationBuilderMock = new Mock<IConfigurationBuilder>();
            configuration = new ConsulSettings
            {
                Host = "http://localhost:8500",
                ServiceName = "TestService"
            };
            mockConfiguration = new Mock<IConfiguration>();
        }

        [Test]
        public void GetConsulSettings_ShouldReturnConsulSettings()
        {
            // Arrange
            var expectedHost = "http://localhost:8500";
            var expectedServiceName = "TestService";
            var expectedServicePort = "5000";
            mockConfiguration.Setup(x => x[ConsulConfiguration.CONSUL_HOST]).Returns(expectedHost);
            mockConfiguration.Setup(x => x[ConsulConfiguration.CONSUL_SERVICE_NAME]).Returns(expectedServiceName);
            mockConfiguration.Setup(x => x[ConsulConfiguration.CONSUL_SERVICE_PORT]).Returns(expectedServicePort);
            // Act
            var consulSettings = ConsulExtension.GetConsulSettings(mockConfiguration.Object);
            // Assert
            Assert.IsNotNull(consulSettings);
            Assert.That(consulSettings.Host, Is.EqualTo(expectedHost));
            Assert.That(consulSettings.ServiceName, Is.EqualTo(expectedServiceName));
            Assert.That(consulSettings.ServicePort, Is.EqualTo(int.Parse(expectedServicePort)));
        }
        [Test]
        public void AddConsulConfiguration_ShouldModifyConfigurationBuilder()
        {
            // Arrange
            var configurationSourceMock = new Mock<IConfigurationSource>();
            var sources = new List<IConfigurationSource> { configurationSourceMock.Object };
            configurationBuilderMock.Setup(cb => cb.Sources).Returns(sources);
            // Act
            configurationBuilderMock.Object.ConfigureConsul(configuration, "Development");
            // Assert
            configurationBuilderMock.Verify(cb => cb.Add(It.IsAny<IConfigurationSource>()), Times.AtLeastOnce);
        }

        [Test]
        public void AddConsulService_ShouldRegisterServices()
        {
            //Arrange
            var consulSettings = new ConsulSettings
            {
                Host = "http://localhost:8500",
                ServiceName = "TestService",
                ServicePort = 5000
            };
            var services = new ServiceCollection();
            // Act
            services.AddConsulService(consulSettings);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var registeredConsulSettings = serviceProvider.GetService<ConsulSettings>();
            var consulClient = serviceProvider.GetService<IConsulClient>();
            Assert.IsNotNull(consulClient);
            Assert.IsInstanceOf<ConsulClient>(consulClient);
        }
    }
}