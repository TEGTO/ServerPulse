using AnalyzerApi.Infrastructure.Models.Statistics;
using EventCommunication;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnalyzerApi.IntegrationTests.Hubs
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class ServerLoadStatisticsHubTests : BaseIntegrationTest
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
            var eventSamples = new List<LoadEvent>();
            string? receivedKey = null;
            ServerLoadStatistics? receivedStatistics = null;

            eventSamples.Add(new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow));
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", new[]
            {
               eventSamples[0]
            });

            eventSamples.Add(new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", new[]
            {
               eventSamples[1]
            });

            await Task.Delay(2000);

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
                eventSamples[1]
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/loadstatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLoadStatistics>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", key, true);

            await Task.Delay(1000);

            // Assert
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent != null &&
                receivedStatistics.LoadMethodStatistics != null &&
                receivedStatistics.LoadMethodStatistics.GetAmount == 1;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.That(receivedStatistics.AmountOfEvents, Is.EqualTo(2));

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[1].Id));
            Assert.That(receivedStatistics.LastEvent.Method, Is.EqualTo(eventSamples[1].Method));

            Assert.IsNotNull(receivedStatistics.LoadMethodStatistics);
            Assert.That(receivedStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(1));
            Assert.That(receivedStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(1));
            Assert.That(receivedStatistics.LoadMethodStatistics.PutAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PatchAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsNewStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            var eventSamples = new List<LoadEvent>();
            string? receivedKey = null;
            ServerLoadStatistics? receivedStatistics = null;

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/loadstatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLoadStatistics>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", key, false);

            await Task.Delay(1000);

            eventSamples.Add(new LoadEvent(key, "/api/resource", "DELETE", 200, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", new[]
            {
               eventSamples[0]
            });

            await Task.Delay(5000);

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
               eventSamples[0]
            });

            await Task.Delay(1000);

            // Assert - Consuming new event
            await Utility.WaitUntil(() =>
            {
                return
                receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent != null &&
                receivedStatistics.LoadMethodStatistics != null &&
                receivedStatistics.LoadMethodStatistics.DeleteAmount == 1;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.That(receivedStatistics.AmountOfEvents, Is.EqualTo(1));

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[0].Id));
            Assert.That(receivedStatistics.LastEvent.Method, Is.EqualTo(eventSamples[0].Method));

            Assert.IsNotNull(receivedStatistics.LoadMethodStatistics);
            Assert.That(receivedStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PutAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PatchAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(1));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsInitialStatisticsAndThenGetsNewStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            var eventSamples = new List<LoadEvent>();
            string? receivedKey = null;
            ServerLoadStatistics? receivedStatistics = null;

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/loadstatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLoadStatistics>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", key, true);

            await Task.Delay(1000);

            // Assert
            await Utility.WaitUntil(() =>
            {
                return
                receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent == null &&
                receivedStatistics.LoadMethodStatistics != null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));
            Assert.IsNotNull(receivedStatistics);

            Assert.IsNull(receivedStatistics.LastEvent);

            Assert.IsNotNull(receivedStatistics.LoadMethodStatistics);
            Assert.That(receivedStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PutAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PatchAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));

            // Act - Adding new statistics
            eventSamples.Add(new LoadEvent(key, "/api/resource", "DELETE", 200, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", new[]
            {
               eventSamples[0]
            });

            await Task.Delay(5000);

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
               eventSamples[0]
            });

            await Task.Delay(2000);

            // Assert - Consuming new event
            await Utility.WaitUntil(() =>
            {
                return
                receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent != null &&
                receivedStatistics.AmountOfEvents == 1 &&
                receivedStatistics.LoadMethodStatistics != null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.That(receivedStatistics.AmountOfEvents, Is.EqualTo(1));

            Assert.IsNotNull(receivedStatistics.LastEvent);
            Assert.That(receivedStatistics.LastEvent.Id, Is.EqualTo(eventSamples[0].Id));
            Assert.That(receivedStatistics.LastEvent.Method, Is.EqualTo(eventSamples[0].Method));

            Assert.IsNotNull(receivedStatistics.LoadMethodStatistics);
            Assert.That(receivedStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PutAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PatchAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(1));
        }

        [Test]
        public async Task StartListen_WrongKey_ClientAddedToGroupAndGetsEmptyStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            var wrongKey = Guid.NewGuid().ToString();
            var eventSamples = new List<LoadEvent>();
            string? receivedKey = null;
            ServerLoadStatistics? receivedStatistics = null;

            eventSamples.Add(new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow));
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", new[]
            {
                eventSamples[0]
            });

            eventSamples.Add(new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", new[]
            {
                eventSamples[0]
            });

            await Task.Delay(5000);

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
                eventSamples[0]
            });

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
                eventSamples[1]
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/loadstatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLoadStatistics>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", wrongKey, true);

            await Task.Delay(1000);

            // Assert
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent == null &&
                receivedStatistics.LoadMethodStatistics != null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(wrongKey));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNull(receivedStatistics.LastEvent);

            Assert.IsNotNull(receivedStatistics.LoadMethodStatistics);
            Assert.That(receivedStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PutAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PatchAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));

            // Act - Adding new statistics
            eventSamples.Add(new LoadEvent(key, "/api/resource", "DELETE", 200, TimeSpan.FromMilliseconds(200), DateTime.UtcNow));
            await SendEventsAsync(LOAD_PROCESS_TOPIC, "", new[]
            {
               eventSamples[2]
            });

            await Task.Delay(5000);

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
               eventSamples[2]
            });

            await Task.Delay(1000);

            // Assert - Gets nothing
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null &&
                receivedStatistics != null &&
                receivedStatistics.LastEvent == null &&
                receivedStatistics.LoadMethodStatistics != null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(wrongKey));

            Assert.IsNotNull(receivedStatistics);

            Assert.IsNull(receivedStatistics.LastEvent);

            Assert.IsNotNull(receivedStatistics.LoadMethodStatistics);
            Assert.That(receivedStatistics.LoadMethodStatistics.GetAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PostAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PutAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.PatchAmount, Is.EqualTo(0));
            Assert.That(receivedStatistics.LoadMethodStatistics.DeleteAmount, Is.EqualTo(0));
        }
    }
}