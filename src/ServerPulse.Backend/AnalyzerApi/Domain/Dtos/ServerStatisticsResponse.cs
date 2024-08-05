﻿namespace AnalyzerApi.Domain.Dtos
{
    public class ServerStatisticsResponse
    {
        public bool IsAlive { get; set; }
        public bool DataExists { get; set; }
        public DateTime? ServerLastStartDateTime { get; set; }
        public TimeSpan? ServerUptime { get; set; }
        public TimeSpan? LastServerUptime { get; set; }
        public DateTime? LastPulseDateTime { get; set; }
    }
}