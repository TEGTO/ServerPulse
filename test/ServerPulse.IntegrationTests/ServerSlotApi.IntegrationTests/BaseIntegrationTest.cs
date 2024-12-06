using Authentication.Token;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ServerSlotApi.Services;

namespace ServerSlotApi.IntegrationTests
{
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        protected HttpClient client;
        protected JwtSettings settings;
        protected Mock<ISlotStatisticsService>? mockSlotStatisticsService;
        private WebAppFactoryWrapper wrapper;
        private WebApplicationFactory<Program> factory;
        private IServiceScope scope;


        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            wrapper = new WebAppFactoryWrapper();

            factory = (await wrapper.GetFactoryAsync()).WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(ISlotStatisticsService));

                    mockSlotStatisticsService = new Mock<ISlotStatisticsService>();

                    services.AddSingleton(mockSlotStatisticsService.Object);
                });
            });

            InitializeServices();
        }

        [OneTimeTearDown]
        public async Task GlobalTearDown()
        {
            scope.Dispose();
            client.Dispose();
            await factory.DisposeAsync();
            await wrapper.DisposeAsync();
        }

        private void InitializeServices()
        {
            scope = factory.Services.CreateScope();
            client = factory.CreateClient();
            settings = factory.Services.GetRequiredService<JwtSettings>();
        }
    }
}