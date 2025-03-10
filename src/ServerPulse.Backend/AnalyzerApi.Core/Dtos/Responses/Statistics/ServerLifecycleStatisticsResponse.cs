﻿namespace AnalyzerApi.Core.Dtos.Responses.Statistics
{
    public class ServerLifecycleStatisticsResponse : BaseStatisticsResponse
    {
        public bool IsAlive { get; set; }
        public bool DataExists { get; set; }
        public DateTime? ServerLastStartDateTimeUTC { get; set; }
        public TimeSpan? ServerUptime { get; set; }
        public TimeSpan? LastServerUptime { get; set; }
        public DateTime? LastPulseDateTimeUTC { get; set; }
    }
}