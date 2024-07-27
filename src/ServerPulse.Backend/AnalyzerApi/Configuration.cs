﻿namespace AnalyzerApi
{
    public class Configuration
    {
        public static string KAFKA_BOOTSTRAP_SERVERS { get; } = "Kafka:BootstrapServers";
        public static string KAFKA_CLIENT_ID { get; } = "Kafka:ClientId";
        public static string KAFKA_GROUP_ID { get; } = "Kafka:GroupId";
        public static string KAFKA_TIMEOUT_IN_MILLISECONDS { get; } = "Kafka:AnalyzerReceiveTimeout";
        public static string KAFKA_ALIVE_TOPIC { get; } = "Kafka:AliveTopic";
        public static string KAFKA_LOAD_TOPIC { get; } = "Kafka:LoadTopic";
        public static string KAFKA_CONFIGURATION_TOPIC { get; } = "Kafka:ConfigurationTopic";
        public static string CURRENT_LOAD_OFFSET_IN_MILLISECONDS { get; } = "LoadOffsetInMilliseconds";
        public static string PULSE_EVENT_INTERVAL_IN_MILLISECONDS { get; } = "PulseEventIntervalInMilliseconds";
    }
}
