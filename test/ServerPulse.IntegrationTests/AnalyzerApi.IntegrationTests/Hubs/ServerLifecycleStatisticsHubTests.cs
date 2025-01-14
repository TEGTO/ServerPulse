using AnalyzerApi.Core.Models.Statistics;
using EventCommunication;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnalyzerApi.IntegrationTests.Hubs
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class ServerLifecycleStatisticsHubTests : BaseIntegrationTest
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
            string? receivedKey = null;
            ServerLifecycleStatistics? receivedStatistics = null;

            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
            {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(60))
            });

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/lifecyclestatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLifecycleStatistics>("ReceiveStatistics", (k, response) =>
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
                return receivedKey != null && receivedStatistics != null && receivedStatistics.IsAlive;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.True(receivedStatistics.IsAlive);
            Assert.True(receivedStatistics.DataExists);
            Assert.That(receivedStatistics.ServerLastStartDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(receivedStatistics.ServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastPulseDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsNewStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            string? receivedKey = null;
            ServerLifecycleStatistics? receivedStatistics = null;

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/lifecyclestatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLifecycleStatistics>("ReceiveStatistics", (k, response) =>
            {
                receivedKey = k;
                receivedStatistics = response;
            });

            await connection.StartAsync();

            // Act
            await connection.SendAsync("StartListen", key, false);

            await Task.Delay(1000);

            // Assert
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null && receivedStatistics != null && !receivedStatistics.IsAlive;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.False(receivedStatistics.IsAlive);
            Assert.False(receivedStatistics.DataExists);
            Assert.That(receivedStatistics.ServerLastStartDateTimeUTC, Is.EqualTo(default));
            Assert.That(receivedStatistics.ServerUptime, Is.EqualTo(default));
            Assert.That(receivedStatistics.LastServerUptime, Is.EqualTo(default));
            Assert.That(receivedStatistics.LastPulseDateTimeUTC, Is.EqualTo(default));

            // Act - Adding new statistics
            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
            {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(6))
            });

            await Task.Delay(5000);

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            // Assert - Gets a new added statistics 
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null && receivedStatistics != null && receivedStatistics.IsAlive;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.True(receivedStatistics.IsAlive);
            Assert.True(receivedStatistics.DataExists);
            Assert.That(receivedStatistics.ServerLastStartDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(receivedStatistics.ServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastPulseDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsInitialStatisticsAndThenGetsNewStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            string? receivedKey = null;
            ServerLifecycleStatistics? receivedStatistics = null;

            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
            {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(60))
            });

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/lifecyclestatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLifecycleStatistics>("ReceiveStatistics", (k, response) =>
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
                return receivedKey != null && receivedStatistics != null && receivedStatistics.IsAlive;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.True(receivedStatistics.IsAlive);
            Assert.True(receivedStatistics.DataExists);
            Assert.That(receivedStatistics.ServerLastStartDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(receivedStatistics.ServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastPulseDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));

            // Act - Adding new statistics
            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
            {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(6))
            });

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(1000);

            // Assert - Gets a new added statistics 
            await Utility.WaitUntil(() =>
            {
                return receivedKey != null && receivedStatistics != null && receivedStatistics.IsAlive;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(key));

            Assert.IsNotNull(receivedStatistics);
            Assert.True(receivedStatistics.IsAlive);
            Assert.True(receivedStatistics.DataExists);
            Assert.That(receivedStatistics.ServerLastStartDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(receivedStatistics.ServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastServerUptime, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(receivedStatistics.LastPulseDateTimeUTC, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)));
        }

        [Test]
        public async Task StartListen_WrongKey_ClientAddedToGroupAndGetsEmptyStatistics()
        {
            // Arrange 
            var key = Guid.NewGuid().ToString();
            var wrongKey = Guid.NewGuid().ToString();
            string? receivedKey = null;
            ServerLifecycleStatistics? receivedStatistics = null;

            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
            {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(60))
            });

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(1000);

            var connection = new HubConnectionBuilder()
            .WithUrl("wss://localhost" + "/lifecyclestatisticshub", options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
            })
            .Build();

            connections.Add(connection);

            connection.On<string, ServerLifecycleStatistics>("ReceiveStatistics", (k, response) =>
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
                return receivedKey != null && receivedStatistics != null;
            }, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));

            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(wrongKey));

            Assert.IsNotNull(receivedStatistics);
            Assert.False(receivedStatistics.IsAlive);
            Assert.False(receivedStatistics.DataExists);
            Assert.That(receivedStatistics.ServerLastStartDateTimeUTC, Is.EqualTo(default));
            Assert.That(receivedStatistics.ServerUptime, Is.EqualTo(default));
            Assert.That(receivedStatistics.LastServerUptime, Is.EqualTo(default));
            Assert.That(receivedStatistics.LastPulseDateTimeUTC, Is.EqualTo(default));

            // Act - Adding new statistics
            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
              {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(60))
            });

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(1000);

            // Assert - Gets nothing
            Assert.IsNotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(wrongKey));

            Assert.IsNotNull(receivedStatistics);
            Assert.False(receivedStatistics.IsAlive);
            Assert.False(receivedStatistics.DataExists);
            Assert.That(receivedStatistics.ServerLastStartDateTimeUTC, Is.EqualTo(default));
            Assert.That(receivedStatistics.ServerUptime, Is.EqualTo(default));
            Assert.That(receivedStatistics.LastServerUptime, Is.EqualTo(default));
            Assert.That(receivedStatistics.LastPulseDateTimeUTC, Is.EqualTo(default));
        }
    }
}
