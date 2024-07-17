using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared;
using Shared.Repositories;

namespace SharedTests
{
    [TestFixture]
    internal class ApplicationBuilderExtensionsTests
    {
        private Mock<IApplicationBuilder> appBuilderMock;
        private Mock<IServiceProvider> serviceProviderMock;
        private Mock<IServiceScope> serviceScopeMock;
        private Mock<IServiceScopeFactory> serviceScopeFactoryMock;
        private Mock<IConfiguration> configurationMock;
        private Mock<ILogger<IApplicationBuilder>> loggerMock;
        private Mock<IDatabaseRepository<DbContext>> repositoryMock;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            appBuilderMock = new Mock<IApplicationBuilder>();
            serviceProviderMock = new Mock<IServiceProvider>();
            serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            configurationMock = new Mock<IConfiguration>();
            loggerMock = new Mock<ILogger<IApplicationBuilder>>();
            repositoryMock = new Mock<IDatabaseRepository<DbContext>>();

            serviceScopeFactoryMock.Setup(factory => factory.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(scope => scope.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceProviderMock.Setup(provider => provider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceProviderMock.Setup(provider => provider.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);
            serviceProviderMock.Setup(provider => provider.GetService(typeof(ILogger<IApplicationBuilder>))).Returns(loggerMock.Object);
            serviceProviderMock.Setup(provider => provider.GetService(typeof(IDatabaseRepository<DbContext>))).Returns(repositoryMock.Object);

            appBuilderMock.Setup(app => app.ApplicationServices).Returns(serviceProviderMock.Object);

            cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ConfigureDatabaseAsyncShouldApplyMigrationsWhenEFCreateDatabaseIsTrue()
        {
            // Arrange
            configurationMock.Setup(config => config["EFCreateDatabase"]).Returns("true");
            // Act
            await appBuilderMock.Object.ConfigureDatabaseAsync<DbContext>(cancellationToken);
            // Assert
            repositoryMock.Verify(repo => repo.MigrateDatabaseAsync(cancellationToken), Times.Once);
        }
        [Test]
        public async Task ConfigureDatabaseAsyncShouldNotApplyMigrationsWhenEFCreateDatabaseIsFalse()
        {
            // Arrange
            configurationMock.Setup(config => config["EFCreateDatabase"]).Returns("false");
            // Act
            await appBuilderMock.Object.ConfigureDatabaseAsync<DbContext>(cancellationToken);
            // Assert
            repositoryMock.Verify(repo => repo.MigrateDatabaseAsync(cancellationToken), Times.Never);
        }
    }
}