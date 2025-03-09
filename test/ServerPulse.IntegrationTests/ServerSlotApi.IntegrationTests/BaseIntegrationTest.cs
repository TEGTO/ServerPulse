using Authentication.Token;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ServerSlotApi.IntegrationTests
{
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        protected HttpClient client;
        protected ITokenHandler tokenHandler;
        private WebAppFactoryWrapper wrapper;
        private WebApplicationFactory<Program> factory;
        private IServiceScope scope;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            wrapper = new WebAppFactoryWrapper();

            factory = (await wrapper.GetFactoryAsync());

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

            tokenHandler = factory.Services.GetRequiredService<ITokenHandler>();
        }
    }
}