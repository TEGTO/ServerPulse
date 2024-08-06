using Consul;
using ConsulUtils.Configuration;
using ConsulUtils.Extension;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;

namespace ConsulUtilsTests
{
    [TestFixture]
    internal class ServiceDiscoveryHostedServiceTests
    {
        private Mock<IConsulClient> consulClientMock;
        private Mock<IHostApplicationLifetime> hostApplicationLifetimeMock;
        private ConsulSettings configuration;
        private ServiceDiscoveryHostedService hostedService;

        [SetUp]
        public void SetUp()
        {
            consulClientMock = new Mock<IConsulClient>();
            hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            configuration = new ConsulSettings
            {
                Host = "http://localhost:8500",
                ServiceName = "TestService",
                ServicePort = 5000
            };

            hostedService = new ServiceDiscoveryHostedService(consulClientMock.Object, configuration, hostApplicationLifetimeMock.Object);
        }

        [Test]
        public async Task StartAsync_ShouldRegisterService()
        {
            // Arrange
            var serviceIp = IPAddress.Loopback.ToString();
            var registrationId = $"{configuration.ServiceName}-{serviceIp}:{configuration.ServicePort}";
            var agentMock = new Mock<IAgentEndpoint>();
            consulClientMock.Setup(client => client.Agent).Returns(agentMock.Object);
            var writeResult = new WriteResult();
            agentMock.Setup(agent => agent.ServiceDeregister(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(writeResult));
            agentMock.Setup(agent => agent.ServiceRegister(It.IsAny<AgentServiceRegistration>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(writeResult));
            // Act
            await hostedService.StartAsync(CancellationToken.None);
            // Assert
            agentMock.Verify(agent => agent.ServiceDeregister(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            agentMock.Verify(agent => agent.ServiceRegister(It.IsAny<AgentServiceRegistration>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task StopAsync_ShouldDeregisterService()
        {
            // Arrange
            var agentMock = new Mock<IAgentEndpoint>();
            consulClientMock.Setup(client => client.Agent).Returns(agentMock.Object);
            var writeResult = new WriteResult();
            agentMock.Setup(agent => agent.ServiceDeregister(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(writeResult));
            // Act
            await hostedService.StopAsync(CancellationToken.None);
            // Assert
            consulClientMock.Verify(client => client.Agent.ServiceDeregister(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}