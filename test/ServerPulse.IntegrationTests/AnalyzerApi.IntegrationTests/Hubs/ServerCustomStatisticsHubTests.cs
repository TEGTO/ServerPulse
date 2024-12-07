using AnalyzerApi.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR.Client;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Hubs
{
    internal class ServerCustomStatisticsHubTests : BaseIntegrationTest
    {
        const string KEY = "validKey";

        private HubConnection connection;
        private readonly List<CustomEvent> eventSamples = new List<CustomEvent>();
        private string? receivedKey;
        private string? receivedStatistics;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            eventSamples.Add(new CustomEvent(KEY, "name1", "desc1"));
            await SendEventsAsync(CUSTOM_TOPIC, KEY, new[]
            {
               eventSamples[0]
            });

            await Task.Delay(1000);

            eventSamples.Add(new CustomEvent(KEY, "name2", "desc2"));
            await SendEventsAsync(CUSTOM_TOPIC, KEY, new[]
            {
               eventSamples[1]
            });

            connection = new HubConnectionBuilder()
                .WithUrl("wss://localhost" + "/customstatisticshub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                })
                .Build();

            connection.On<string, string>("ReceiveStatistics", (key, serializedStatistics) =>
            {
                receivedKey = key;
                receivedStatistics = serializedStatistics;
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
        public async Task StartListen_ValidKey_ClientAddedToGroupAndStartsConsumingStatistics()
        {
            // Act
            await connection.SendAsync("StartListen", KEY);

            await Task.Delay(2000);

            // Assert
            Assert.NotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(KEY));
            Assert.NotNull(receivedStatistics);

            var statistics = JsonSerializer.Deserialize<ServerCustomStatistics>(receivedStatistics);
            Assert.NotNull(statistics);

            Assert.True(statistics.IsInitial);
            Assert.NotNull(statistics.LastEvent);
            Assert.That(statistics.LastEvent.Id, Is.EqualTo(eventSamples[1].Id));
            Assert.That(statistics.LastEvent.Name, Is.EqualTo(eventSamples[1].Name));

            // Act - Adding new statistics
            eventSamples.Add(new CustomEvent(KEY, "name3", "desc3"));
            await SendEventsAsync(CUSTOM_TOPIC, KEY, new[]
            {
               eventSamples[2]
            });

            // Assert - Consuming new added statistics 
            Assert.NotNull(receivedKey);
            Assert.That(receivedKey, Is.EqualTo(KEY));
            Assert.NotNull(receivedStatistics);

            statistics = JsonSerializer.Deserialize<ServerCustomStatistics>(receivedStatistics);
            Assert.NotNull(statistics);

            Assert.False(statistics.IsInitial);
            Assert.NotNull(statistics.LastEvent);
            Assert.That(statistics.LastEvent.Id, Is.EqualTo(eventSamples[2].Id));
            Assert.That(statistics.LastEvent.Name, Is.EqualTo(eventSamples[2].Name));
        }
    }
}