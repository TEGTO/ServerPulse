﻿namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class ServerStatisticsResponse
    {
        public bool IsAlive { get; set; }
        public bool DataExists { get; set; }
        public DateTime? ServerLastStartDateTimeUTC { get; set; }
        public TimeSpan? ServerUptime { get; set; }
        public TimeSpan? LastServerUptime { get; set; }
        public DateTime? LastPulseDateTimeUTC { get; set; }
        public DateTime CollectedDateUTC { get; set; }
        public bool IsInitial { get; set; }
    }
}