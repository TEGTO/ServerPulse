using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ServerMonitorApi.Infrastructure.Services;
using Shared;

namespace ServerMonitorApi.IntegrationTests
{
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        protected const string ALIVE_TOPIC = "AliveTopic_";
        protected const string CONFIGURATION_TOPIC = "ConfigurationTopic_";
        protected const string LOAD_TOPIC = "LoadTopic_";
        protected const string LOAD_PROCESS_TOPIC = "LoadEventProcessTopic";
        protected const string CUSTOM_TOPIC = "CustomEventTopic_";
        private const int TIMEOUT_IN_MILLISECONDS = 5000;

        protected HttpClient client;
        protected IMessageConsumer messageConsumer;
        protected Mock<ISlotKeyChecker>? mockSlotKeyChecker;
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
                    services.RemoveAll(typeof(ISlotKeyChecker));

                    mockSlotKeyChecker = new Mock<ISlotKeyChecker>();
                    mockSlotKeyChecker.Setup(x => x.CheckSlotKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

                    services.AddSingleton(mockSlotKeyChecker.Object);
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

        protected async Task<T?> ReceiveLastTopicEventAsync<T>(string topic, string key) where T : BaseEvent
        {
            var response = await messageConsumer.GetLastTopicMessageAsync(topic + key, TIMEOUT_IN_MILLISECONDS, CancellationToken.None);

            if (response != null)
            {
                response.Message.TryToDeserialize(out T? ev);
                return ev;
            }

            return null;
        }

        private void InitializeServices()
        {
            scope = factory.Services.CreateScope();
            client = factory.CreateClient();
            messageConsumer = factory.Services.GetRequiredService<IMessageConsumer>();
        }
    }
}