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
        private EmailSettings expectedSettings;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();

            expectedSettings = new EmailSettings
            {
                ConnectionString = "SomeConnectionString",
                SenderAddress = "some@email.com",
            };

            var inMemorySettings = new Dictionary<string, string>
            {
                { $"{EmailSettings.SETTINGS_SECTION}:{nameof(EmailSettings.ConnectionString)}", expectedSettings.ConnectionString },
                { $"{EmailSettings.SETTINGS_SECTION}:{nameof(EmailSettings.SenderAddress)}", expectedSettings.SenderAddress },
            };

            configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

            services.AddSingleton(configuration);
        }

        [Test]
        public void AddEmailService_ShouldRegisterEmailSettings()
        {
            // Act
            services.AddEmailService(configuration);

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetService<IOptions<EmailSettings>>();
            Assert.That(options, Is.Not.Null);
            var emailSettings = options.Value;

            var emailClientWrapper = serviceProvider.GetService<IEmailClientWrapper>();
            var emailSender = serviceProvider.GetService<IEmailSender>();

            // Assert
            Assert.That(emailSettings, Is.Not.Null);
            Assert.That(emailClientWrapper, Is.Not.Null);
            Assert.That(emailSender, Is.Not.Null);

            Assert.That(emailSettings.ConnectionString, Is.EqualTo(expectedSettings.ConnectionString));
            Assert.That(emailSettings.SenderAddress, Is.EqualTo(expectedSettings.SenderAddress));
        }
    }
}