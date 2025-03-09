using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EmailControl.Tests
{
    [TestFixture]
    internal class ServiceCollectionExtensionsTests
    {
        private IServiceCollection services;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();
        }

        [Test]
        public void AddEmailService_ShouldRegisterAzureEmail()
        {
            // Arrange
            var azureExpectedSettings = new AzureEmailSettings
            {
                ConnectionString = "SomeConnectionString",
                SenderAddress = "some@email.com",
            };

            var inMemorySettings = new Dictionary<string, string>
            {
                { $"{AzureEmailSettings.SETTINGS_SECTION}:{nameof(AzureEmailSettings.ConnectionString)}", azureExpectedSettings.ConnectionString },
                { $"{AzureEmailSettings.SETTINGS_SECTION}:{nameof(AzureEmailSettings.SenderAddress)}", azureExpectedSettings.SenderAddress },
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

            services.AddSingleton(configuration);

            // Act
            services.AddEmailService(configuration, serviceType: EmailServiceType.Azure, enableFeature: true);

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetService<IOptions<AzureEmailSettings>>();
            Assert.That(options, Is.Not.Null);
            var emailSettings = options.Value;

            var azureEmailClient = serviceProvider.GetService<IAzureEmailClientFacade>();
            var emailSender = serviceProvider.GetService<IEmailSender>();

            // Assert
            Assert.That(emailSettings, Is.Not.Null);
            Assert.That(azureEmailClient, Is.Not.Null);
            Assert.That(emailSender, Is.Not.Null);

            Assert.That(emailSender.GetType(), Is.EqualTo(typeof(AzureEmailSender)));

            Assert.That(emailSettings.ConnectionString, Is.EqualTo(azureExpectedSettings.ConnectionString));
            Assert.That(emailSettings.SenderAddress, Is.EqualTo(azureExpectedSettings.SenderAddress));
        }

        [Test]
        public void AddEmailService_ShouldRegisterAwsEmail()
        {
            // Arrange
            var awsExpectedSettings = new AwsEmailSettings
            {
                SenderAddress = "some@email.com",
                AccessKey = "SomeAccess",
                SecretKey = "SomeSecret",
                Region = "us-west-1",
            };

            var inMemorySettings = new Dictionary<string, string>
            {
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.SenderAddress)}", awsExpectedSettings.SenderAddress },
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.AccessKey)}", awsExpectedSettings.AccessKey },
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.SecretKey)}", awsExpectedSettings.SecretKey },
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.Region)}", awsExpectedSettings.Region },
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

            services.AddSingleton(configuration);

            // Act
            services.AddEmailService(configuration, serviceType: EmailServiceType.AWS, enableFeature: true);

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetService<IOptions<AwsEmailSettings>>();
            Assert.That(options, Is.Not.Null);
            var emailSettings = options.Value;

            var awsEmailClient = serviceProvider.GetService<IAwsEmailClientFacade>();
            var emailSender = serviceProvider.GetService<IEmailSender>();

            // Assert
            Assert.That(emailSettings, Is.Not.Null);
            Assert.That(awsEmailClient, Is.Not.Null);
            Assert.That(emailSender, Is.Not.Null);

            Assert.That(emailSender.GetType(), Is.EqualTo(typeof(AwsEmailSender)));

            Assert.That(emailSettings.SenderAddress, Is.EqualTo(awsExpectedSettings.SenderAddress));
        }
    }
}