using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polly.Registry;

namespace Resilience.Tests
{
    [TestFixture]
    internal class ServiceCollectionExtensionsTests
    {
        private IServiceCollection services;
        private Mock<IConfiguration> configurationMock;

        [SetUp]
        public void SetUp()
        {
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(x => x.GetSection(It.IsAny<string>()))
                .Returns(new Mock<IConfigurationSection>().Object);

            services = new ServiceCollection();
        }

        [Test]
        public void AddSharedFluentValidation_ShouldRegisterFluentValidationServices()
        {
            //Act
            services.AddDefaultResiliencePipeline(configurationMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            //Assert
            var provider = serviceProvider.GetService<ResiliencePipelineProvider<string>>();
            Assert.NotNull(provider);

            var pipeline = provider.GetPipeline("Default");
            Assert.NotNull(pipeline);
        }
    }
}