using FluentValidation;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Configurations;
using Shared.Helpers;
using Swashbuckle.AspNetCore.Swagger;

namespace Shared.Tests
{
    [TestFixture]
    internal class ServiceCollectionExtensionsTests
    {
        private IServiceCollection services;
        private Mock<IConfiguration> configurationMock;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();
            configurationMock = new Mock<IConfiguration>();
        }

        [Test]
        public void AddSharedFluentValidation_ShouldRegisterFluentValidationServices()
        {
            //Act
            services.AddSharedFluentValidation(typeof(ServiceCollectionExtensionsTests));
            var serviceProvider = services.BuildServiceProvider();

            //Assert
            var validator = serviceProvider.GetService<IValidator<MockEntity>>();

            Assert.NotNull(validator);
        }

        [TestCase("http://example.com,http://test.com", true, 2)]
        [TestCase("", true, 0)]
        [TestCase("http://localhost,http://prod.com", false, 2)]
        public void AddApplicationCors_RegistersCorsWithCorrectOrigins(string allowedOrigins, bool isDevelopment, int expectedCount)
        {
            // Arrange
            configurationMock.Setup(c => c[SharedConfiguration.ALLOWED_CORS_ORIGINS]).Returns(allowedOrigins);

            services.AddLogging();
            services.AddRouting();

            // Act
            services.AddApplicationCors(configurationMock.Object, "TestPolicy", isDevelopment);

            var serviceProvider = services.BuildServiceProvider();
            var corsService = serviceProvider.GetRequiredService<ICorsService>();

            // Assert
            Assert.IsNotNull(corsService);
            Assert.That(services.Any(s => s.ServiceType == typeof(ICorsService)));

            var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();
            var policy = policyProvider.GetPolicyAsync(new DefaultHttpContext(), "TestPolicy").Result;

            Assert.That(policy?.Origins.Count, Is.EqualTo(expectedCount));
        }

        [Test]
        public void AddApplicationCors_DevelopmentMode_AllowsLocalhost()
        {
            // Arrange
            configurationMock.Setup(c => c[SharedConfiguration.ALLOWED_CORS_ORIGINS]).Returns("http://example.com");

            services.AddLogging();
            services.AddRouting();

            // Act
            services.AddApplicationCors(configurationMock.Object, "DevPolicy", true);

            var serviceProvider = services.BuildServiceProvider();
            var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();
            var policy = policyProvider.GetPolicyAsync(new DefaultHttpContext(), "DevPolicy").Result;

            // Assert
            Assert.IsNotNull(policy);
            Assert.IsTrue(policy.IsOriginAllowed("http://localhost"));
        }

        [Test]
        public void AddCustomHttpClientServiceWithResilience_RegistersHttpClientAndDependencies()
        {
            // Act
            services.AddCustomHttpClientServiceWithResilience(configurationMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            Assert.That(services.Any(s => s.ServiceType == typeof(IHttpHelper)));
            Assert.IsNotNull(serviceProvider.GetRequiredService<IHttpHelper>());

            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            Assert.IsNotNull(factory);

            var client = factory.CreateClient(SharedConfiguration.HTTP_CLIENT_RESILIENCE_PIPELINE);
            Assert.IsNotNull(client);
        }

        [Test]
        public void AddSwagger_RegistersSwaggerWithSecurityDefinitions()
        {
            // Arrange
            var title = "Test API";

            // Act
            services.AddSwagger(title);

            services.BuildServiceProvider();

            // Assert
            Assert.That(services.Any(s => s.ServiceType == typeof(ISwaggerProvider)));
        }
    }

    public class MockValidator : AbstractValidator<MockEntity>
    {
    }
    public class MockEntity
    {
        public string? Id { get; set; }
    }
}