namespace AnalyzerApi.Infrastructure.Configuration
{
    public static class ConfigurationKeys
    {
        public static string STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS { get; } = "StatisticsCollectIntervalInMilliseconds";
        public static string MIN_STATISTICS_TIMESPAN_IN_SECONDS { get; } = "MinimumStatisticsTimeSpanInSeconds";
        public static string MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA { get; } = "MaxEventAmountToGetInSlotData";
        public static string MAX_EVENT_AMOUNT_PER_REQUEST { get; } = "MaxEventAmountToReadPerRequest";
    }
}