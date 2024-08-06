using ConsulUtils.Configuration;
using ConsulUtils.Extension;
using Microsoft.Extensions.Configuration;
using Moq;


namespace ConsulUtilsTests
{
    [TestFixture]
    internal class ConsulConfigurationExtensionTests
    {
        private Mock<IConfigurationBuilder> configurationBuilderMock;
        private ConsulSettings configuration;

        [SetUp]
        public void SetUp()
        {
            configurationBuilderMock = new Mock<IConfigurationBuilder>();
            configuration = new ConsulSettings
            {
                Host = "http://localhost:8500",
                ServiceName = "TestService"
            };
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
    }
}
