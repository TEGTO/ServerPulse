﻿using AnalyzerApi.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR.Client;
using ServerPulse.EventCommunication.Events;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Hubs
{
    [TestFixture]
    internal class ServerStatisticsHubTests : BaseIntegrationTest
    {
        private HubConnection connection;
        private readonly ConcurrentDictionary<string, string> receivedStatistics = new ConcurrentDictionary<string, string>();

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("wss://localhost" + "/statisticshub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                })
                .Build();

            connection.On<string, string>("ReceiveStatistics", (key, serializedStatistics) =>
            {
                receivedStatistics[key] = serializedStatistics;
            });

            await connection.StartAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsInitialStatistics()
        {
            //Arrange
            var key = "key1";

            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
            {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(60))
            });

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(1000);

            // Act
            await connection.SendAsync("StartListen", key);

            await Task.Delay(2000);

            // Assert
            Assert.True(receivedStatistics.ContainsKey(key));
            Assert.NotNull(receivedStatistics[key]);

            var statistics = JsonSerializer.Deserialize<ServerStatistics>(receivedStatistics[key]);
            Assert.NotNull(statistics);

            Assert.True(statistics.IsInitial);
            Assert.True(statistics.IsAlive);
            Assert.True(statistics.DataExists);
            Assert.That(statistics.ServerLastStartDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(statistics.ServerUptime, Is.EqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(statistics.LastServerUptime, Is.EqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(statistics.LastPulseDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndDoesNotGetInitialStatistics()
        {
            //Arrange
            var key = "key2";

            await Task.Delay(1000);

            // Act
            await connection.SendAsync("StartListen", key);

            await Task.Delay(2000);

            // Assert
            Assert.False(receivedStatistics.ContainsKey(key));
        }

        [Test]
        public async Task StartListen_ValidKey_ClientAddedToGroupAndGetsStatistics()
        {
            //Arrange
            var key = "key3";
            await SendEventsAsync(CONFIGURATION_TOPIC, key, new[]
            {
                new ConfigurationEvent(key, TimeSpan.FromSeconds(5))
            });

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
              new PulseEvent(key, true)
            });

            await Task.Delay(1000);

            // Act
            await connection.SendAsync("StartListen", key);

            await Task.Delay(7000);

            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(2000);

            // Assert
            Assert.True(receivedStatistics.ContainsKey(key));
            Assert.NotNull(receivedStatistics[key]);

            var statistics = JsonSerializer.Deserialize<ServerStatistics>(receivedStatistics[key]);
            Assert.NotNull(statistics);

            Assert.False(statistics.IsInitial);
            Assert.True(statistics.IsAlive);
            Assert.True(statistics.DataExists);
            Assert.That(statistics.ServerLastStartDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(statistics.ServerUptime, Is.GreaterThan(TimeSpan.FromSeconds(0)));
            Assert.That(statistics.LastServerUptime, Is.GreaterThan(TimeSpan.FromSeconds(0)));
            Assert.That(statistics.LastPulseDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));

            //Act - Wait for the next statistics
            await Task.Delay(5000);

            // Assert - Consuming the next statistics
            Assert.True(receivedStatistics.ContainsKey(key));
            Assert.NotNull(receivedStatistics[key]);

            statistics = JsonSerializer.Deserialize<ServerStatistics>(receivedStatistics[key]);
            Assert.NotNull(statistics);

            Assert.False(statistics.IsInitial);
            Assert.False(statistics.IsAlive);
            Assert.True(statistics.DataExists);
            Assert.That(statistics.ServerLastStartDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(statistics.ServerUptime, Is.Null);
            Assert.That(statistics.LastServerUptime, Is.GreaterThan(TimeSpan.FromSeconds(0)));
            Assert.That(statistics.LastPulseDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));

            //Act - Adding new event
            await SendEventsAsync(ALIVE_TOPIC, key, new[]
            {
                new PulseEvent(key, true)
            });

            await Task.Delay(7000);

            // Assert - Consuming new statistics with the added event
            Assert.True(receivedStatistics.ContainsKey(key));
            Assert.NotNull(receivedStatistics[key]);

            statistics = JsonSerializer.Deserialize<ServerStatistics>(receivedStatistics[key]);
            Assert.NotNull(statistics);
            Assert.False(statistics.IsInitial);
            Assert.True(statistics.IsAlive);
            Assert.True(statistics.DataExists);
            Assert.That(statistics.ServerLastStartDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
            Assert.That(statistics.ServerUptime, Is.EqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(statistics.LastServerUptime, Is.EqualTo(TimeSpan.FromSeconds(0)));
            Assert.That(statistics.LastPulseDateTimeUTC, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
        }
    }
}
