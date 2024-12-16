using EventCommunication;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests
{
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        protected const string ALIVE_TOPIC = "AliveTopic_";
        protected const string CONFIGURATION_TOPIC = "ConfigurationTopic_";
        protected const string LOAD_TOPIC = "LoadTopic_";
        protected const string LOAD_PROCESS_TOPIC = "LoadEventProcessTopic";
        protected const string CUSTOM_TOPIC = "CustomEventTopic_";
        protected const string LOAD_METHOD_STATISTICS_TOPIC = "LoadMethodStatisticsTopic_";
        private const int TIMEOUT_IN_MILLISECONDS = 5000;

        protected HttpClient client;
        protected TestServer server;
        protected IMessageConsumer messageConsumer;
        protected IMessageProducer producer;
        private WebAppFactoryWrapper wrapper;
        private WebApplicationFactory<Program> factory;
        private IServiceScope scope;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            wrapper = new WebAppFactoryWrapper();
            factory = await wrapper.GetFactoryAsync();
            InitializeServices();
        }

        [OneTimeTearDown]
        public async Task GlobalTearDown()
        {
            scope.Dispose();
            client.Dispose();
            await wrapper.DisposeAsync();
        }

        protected async Task<T?> ReceiveLastObjectFromTopicAsync<T>(string topic, string key) where T : class
        {
            var response = await messageConsumer.ReadLastTopicMessageAsync(topic + key, TIMEOUT_IN_MILLISECONDS, CancellationToken.None);
            if (response != null)
            {
                response.Message.TryToDeserialize(out T? ev);
                return ev;
            }
            return null;
        }

        protected async Task SendCustomEventsAsync(string topic, string key, string[] serializedEvents)
        {
            foreach (var ev in serializedEvents)
            {
                await producer.ProduceAsync(topic + key, ev, CancellationToken.None);
            }
        }

        protected async Task SendEventsAsync<T>(string topic, string key, T[] events) where T : BaseEvent
        {
            await Parallel.ForEachAsync(events, async (ev, ct) =>
            {
                if (!string.IsNullOrEmpty(topic + key))
                {
                    var message = JsonSerializer.Serialize(ev);
                    await producer.ProduceAsync(topic + key, message, CancellationToken.None);
                }
            });
        }

        private void InitializeServices()
        {
            scope = factory.Services.CreateScope();
            client = factory.CreateClient();
            server = factory.Server;
            messageConsumer = factory.Services.GetRequiredService<IMessageConsumer>();
            producer = factory.Services.GetRequiredService<IMessageProducer>();
        }
    }
}