using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using EventCommunication;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnalyzerApi.IntegrationTests.Hubs
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class ServerCustomStatisticsHubTests : BaseIntegrationTest
    {
        private readonly List<HubConnection> connections = new List<HubConnection>();

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            foreach (var connection in connections)
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
            }
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsInitial()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            var eventSamples = new List<CustomEvent>();
            string? receivedKey = null;
            ServerCustomStatisticsResponse? receivedStatistics = null;

            eventSamples.Add(new CustomEvent(key, "name1", "desc1"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            eventSamples.Add(new CustomEvent(key, "name2", "desc2"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[1]
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/customstatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerCustomStatisticsResponse>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", key);

            // Assert
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null && receivedStatistics != null && receivedStatistics.LastEvent != null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[1].Id));
            Assert.That(receivedStatistics.LastEvent.Name, Is.EqualTo(eventSamples[1].Name));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsInitialStatisticsAndThenGetsNewStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            var eventSamples = new List<CustomEvent>();
            string? receivedKey = null;
            ServerCustomStatisticsResponse? receivedStatistics = null;

            eventSamples.Add(new CustomEvent(key, "name1", "desc1"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            eventSamples.Add(new CustomEvent(key, "name2", "desc2"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[1]
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/customstatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerCustomStatisticsResponse>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", key);

            // Assert
            await Task.Delay(3000);

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));
            Assert.IsNotNull(receivedStatistics);

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[1].Id));
            Assert.That(receivedStatistics.LastEvent.Name, Is.EqualTo(eventSamples[1].Name));

            // Act - Adding new statistics
            eventSamples.Add(new CustomEvent(key, "name3", "desc3"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
               eventSamples[2]
            });

            await Task.Delay(1000);

            // Assert - Gets a new added statistics 
            await Utility.WaitUntil(() =>
            {
                return
                receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent != null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[2].Id));
            Assert.That(receivedStatistics.LastEvent.Name, Is.EqualTo(eventSamples[2].Name));
        }

        [Test]
        public async Task StartListen_WrongKey_ClientAddedToGroupAndGetsEmptyStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            var wrongKey = Guid.NewGuid().ToString();
            var eventSamples = new List<CustomEvent>();
            string? receivedKey = null;
            ServerCustomStatisticsResponse? receivedStatistics = null;

            eventSamples.Add(new CustomEvent(key, "name1", "desc1"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            eventSamples.Add(new CustomEvent(key, "name2", "desc2"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[1]
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/customstatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerCustomStatisticsResponse>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", wrongKey);

            // Assert
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null && receivedStatistics != null && receivedStatistics.LastEvent == null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(wrongKey));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNull(receivedStatistics.LastEvent);

            // Act - Adding new statistics
            eventSamples.Add(new CustomEvent(key, "name3", "desc3"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[2]
            });

            await Task.Delay(1000);

            // Assert - Gets nothing
            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(wrongKey));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNull(receivedStatistics.LastEvent);
        }
    }
}