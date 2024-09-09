﻿using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace ServerMonitorApi.Services
{
    public class EventSender : IEventSender
    {
        private readonly IMessageProducer producer;
        private readonly string pulseTopic;
        private readonly string configurationTopic;
        private readonly string loadTopic;
        private readonly string customTopic;

        public EventSender(IMessageProducer producer, IConfiguration configuration)
        {
            this.producer = producer;
            pulseTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
            customTopic = configuration[Configuration.KAFKA_CUSTOM_TOPIC]!;
        }

        public async Task SendCustomEventsAsync(string key, string[] serializedEvents, CancellationToken cancellationToken)
        {
            var topic = GetTopic(customTopic, key);
            foreach (var ev in serializedEvents)
            {
                await producer.ProduceAsync(topic, ev, cancellationToken);
            }
        }
        public async Task SendEventsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent
        {
            await Parallel.ForEachAsync(events, cancellationToken, async (ev, ct) =>
            {
                string topic = GetTopic(ev);
                if (!string.IsNullOrEmpty(topic))
                {
                    var message = JsonSerializer.Serialize(ev);
                    await producer.ProduceAsync(topic, message, cancellationToken);
                }
            });
        }
        private string GetTopic(BaseEvent ev)
        {
            return ev switch
            {
                PulseEvent _ => GetTopic(pulseTopic, ev.Key),
                ConfigurationEvent _ => GetTopic(configurationTopic, ev.Key),
                LoadEvent _ => GetTopic(loadTopic, ev.Key),
                _ => string.Empty
            };
        }
        private string GetTopic(string topic, string key)
        {
            return topic + key;
        }
    }
}