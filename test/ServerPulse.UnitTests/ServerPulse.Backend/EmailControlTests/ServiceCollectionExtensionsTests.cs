#define AWS
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EmailControl.Tests
{
    [TestFixture]
    internal class ServiceCollectionExtensionsTests
    {
        private IServiceCollection services;
        private IConfiguration configuration;
#if Azure
        private AzureEmailSettings expectedSettings;
#endif
#if AWS
        private AwsEmailSettings expectedSettings;
#endif

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();

#if Azure
            expectedSettings = new AzureEmailSettings
            {
                ConnectionString = "SomeConnectionString",
                SenderAddress = "some@email.com",
            };

            var inMemorySettings = new Dictionary<string, string>
            {
                { $"{AzureEmailSettings.SETTINGS_SECTION}:{nameof(AzureEmailSettings.ConnectionString)}", expectedSettings.ConnectionString },
                { $"{AzureEmailSettings.SETTINGS_SECTION}:{nameof(AzureEmailSettings.SenderAddress)}", expectedSettings.SenderAddress },
            };
#endif
#if AWS
            expectedSettings = new AwsEmailSettings
            {
                SenderAddress = "some@email.com",
                AccessKey = "SomeAccess",
                SecretKey = "SomeSecret",
                Region = "us-west-1",
            };

            var inMemorySettings = new Dictionary<string, string>
            {
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.SenderAddress)}", expectedSettings.SenderAddress },
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.AccessKey)}", expectedSettings.AccessKey },
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.SecretKey)}", expectedSettings.SecretKey },
                { $"{AwsEmailSettings.SETTINGS_SECTION}:{nameof(AwsEmailSettings.Region)}", expectedSettings.Region },
            };
#endif

            configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

            services.AddSingleton(configuration);
        }

#if Azure
        [Test]
        public void AddEmailService_ShouldRegisterAzureEmail()
        {
            // Act
            services.AddEmailService(configuration);

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

            Assert.That(emailSettings.ConnectionString, Is.EqualTo(expectedSettings.ConnectionString));
            Assert.That(emailSettings.SenderAddress, Is.EqualTo(expectedSettings.SenderAddress));
        }
#endif
#if AWS
        [Test]
        public void AddEmailService_ShouldRegisterAwsEmail()
        {
            // Act
            services.AddEmailService(configuration);

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

            Assert.That(emailSettings.SenderAddress, Is.EqualTo(expectedSettings.SenderAddress));
        }
#endif
    }
}