using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using EventCommunication;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnalyzerApi.IntegrationTests.Hubs
{
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
            var key = "key1";
            var eventSamples = new List<CustomEvent>();
            string? receivedKey = null;
            ServerCustomStatisticsResponse? receivedStatistics = null;

            eventSamples.Add(new CustomEvent(key, "name1", "desc1"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            await Task.Delay(1000);

            eventSamples.Add(new CustomEvent(key, "name2", "desc2"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[1]
            });

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
            }, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(1000));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[1].Id));
            Assert.That(receivedStatistics.LastEvent.Name, Is.EqualTo(eventSamples[1].Name));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsNewStatistics()
        {
            // Arrange 
            var key = "key2";
            var eventSamples = new List<CustomEvent>();
            string? receivedKey = null;
            ServerCustomStatisticsResponse? receivedStatistics = null;

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
                return receivedKey != null && receivedStatistics != null && receivedStatistics.LastEvent == null;
            }, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(1000));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNull(receivedStatistics.LastEvent);

            // Act - Adding new statistics
            eventSamples.Add(new CustomEvent(key, "name3", "desc3"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            // Assert - Gets a new added statistics 
            await Utility.WaitUntil(() =>
            {
                return
                receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent != null;
            }, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(1000));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[0].Id));
            Assert.That(receivedStatistics.LastEvent.Name, Is.EqualTo(eventSamples[0].Name));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsInitialStatisticsAndThenGetsNewStatistics()
        {
            // Arrange 
            var key = "key3";
            var eventSamples = new List<CustomEvent>();
            string? receivedKey = null;
            ServerCustomStatisticsResponse? receivedStatistics = null;

            eventSamples.Add(new CustomEvent(key, "name1", "desc1"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            await Task.Delay(1000);

            eventSamples.Add(new CustomEvent(key, "name2", "desc2"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[1]
            });

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

            // Assert - Gets a new added statistics 
            await Utility.WaitUntil(() =>
            {
                return
                receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent != null;
            }, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(3000));

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
            var key = "key4";
            var wrongKey = "wrong-key";
            var eventSamples = new List<CustomEvent>();
            string? receivedKey = null;
            ServerCustomStatisticsResponse? receivedStatistics = null;

            eventSamples.Add(new CustomEvent(key, "name1", "desc1"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            await Task.Delay(1000);

            eventSamples.Add(new CustomEvent(key, "name2", "desc2"));
            await SendEventsAsync(CUSTOM_TOPIC, key, new[]
            {
                eventSamples[1]
            });

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
            }, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(1000));

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

            // Assert - Gets nothing
            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(wrongKey));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNull(receivedStatistics.LastEvent);
        }
    }
}