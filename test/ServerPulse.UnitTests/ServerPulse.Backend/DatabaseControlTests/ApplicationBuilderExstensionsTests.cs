using DatabaseControl.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;

namespace DatabaseControl.Tests
{
    [TestFixture]
    internal class ApplicationBuilderExtensionsTests
    {
        private Mock<IApplicationBuilder> appBuilderMock;
        private Mock<IServiceProvider> serviceProviderMock;
        private Mock<IServiceScope> serviceScopeMock;
        private Mock<IServiceScopeFactory> serviceScopeFactoryMock;
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
            loggerMock = new Mock<ILogger<IApplicationBuilder>>();
            repositoryMock = new Mock<IDatabaseRepository<DbContext>>();

            serviceScopeFactoryMock.Setup(factory => factory.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(scope => scope.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceProviderMock.Setup(provider => provider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceProviderMock.Setup(provider => provider.GetService(typeof(ILogger<IApplicationBuilder>))).Returns(loggerMock.Object);
            serviceProviderMock.Setup(provider => provider.GetService(typeof(IDatabaseRepository<DbContext>))).Returns(repositoryMock.Object);

            appBuilderMock.Setup(app => app.ApplicationServices).Returns(serviceProviderMock.Object);

            cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ConfigureDatabaseAsync_ShouldApplyMigrations()
        {
            // Act
            await appBuilderMock.Object.ConfigureDatabaseAsync<DbContext>(cancellationToken);

            // Assert
            repositoryMock.Verify(repo => repo.MigrateDatabaseAsync(cancellationToken), Times.Once);
            loggerMock.Verify(x => x.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()
               ), Times.Exactly(2));
        }
        [Test]
        public async Task ConfigureDatabaseAsync_ShouldCallLoggerOnException()
        {
            //Assert
            var exception = new InvalidOperationException("Migration is failed!");
            repositoryMock.Setup(x => x.MigrateDatabaseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await appBuilderMock.Object.ConfigureDatabaseAsync<DbContext>(cancellationToken);

            // Assert
            repositoryMock.Verify(repo => repo.MigrateDatabaseAsync(cancellationToken), Times.Once);
            loggerMock.Verify(x => x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               exception,
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()
               ), Times.Once);
        }
    }
}