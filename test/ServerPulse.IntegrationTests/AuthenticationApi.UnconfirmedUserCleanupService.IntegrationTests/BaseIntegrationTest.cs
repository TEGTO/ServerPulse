using Authentication.Token;
using AuthenticationApi.Infrastructure;
using BackgroundTask;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using IEmailSender = EmailControl.IEmailSender;

namespace AuthenticationApi.UnconfirmedUserCleanupService.IntegrationTests
{
    internal abstract class BaseIntegrationTest
    {
        protected HttpClient client;
        protected JwtSettings settings;
        protected UserManager<User> userManager;
        protected bool isConfirmEmailEnabled;
        private WebAppFactoryWrapper wrapper;
        private WebApplicationFactory<Program> factory;
        private IServiceScope scope;
        protected Mock<IBackgroundJobClient>? mockBackgroundJobClient;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            wrapper = new WebAppFactoryWrapper();
            factory = (await wrapper.GetFactoryAsync()).WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IEmailSender));
                    services.RemoveAll(typeof(IBackgroundJobClient));

                    var mockEmailSender = new Mock<IEmailSender>();

                    mockBackgroundJobClient = new Mock<IBackgroundJobClient>();


                    services.AddScoped(_ => mockEmailSender.Object);
                    services.AddScoped(_ => mockBackgroundJobClient.Object);
                });
            });

            InitializeServices();
        }

        [OneTimeTearDown]
        public async Task GlobalTearDown()
        {
            scope.Dispose();
            client.Dispose();
            userManager.Dispose();
            await factory.DisposeAsync();
            await wrapper.DisposeAsync();
        }

        private void InitializeServices()
        {
            scope = factory.Services.CreateScope();
            client = factory.CreateClient();
            settings = factory.Services.GetRequiredService<IOptions<JwtSettings>>().Value;

            var scopedServices = scope.ServiceProvider;
            userManager = scopedServices.GetRequiredService<UserManager<User>>();

            var configuration = factory.Services.GetService<IConfiguration>();
            if (configuration != null)
            {
                isConfirmEmailEnabled = bool.Parse(configuration[$"FeatureManagement:{ConfigurationKeys.REQUIRE_EMAIL_CONFIRMATION}"]!);
            }
        }
    }
}